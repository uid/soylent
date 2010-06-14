// imports
eval(read("../../library/patch.js"));
eval(read("../../library/hit_utils.js"));
eval(read("../../library/diff_match_patch_uncompressed.js"));

var buffer_redundancy = 2;	// number of extra assignments to create so that they don't get squatted.
var wait_time = 20 * 60 * 1000;	// seconds

var search_reward = 0.06;
var search_redundancy = 10;
var search_minimum_agreement = 0.20

var edit_reward = 0.08;
var edit_redundancy = 5;

var verify_redundancy = 5;
var verify_reward = 0.04;

var debug = false;
var time_bounded = true;
var rejectedWorkers = []

var shit_list = ["A521UZVFXBVWZ"]

if (debug)
{
	search_redundancy = 1;
	edit_redundancy = 1;
	verify_redundancy = 1;
	paragraphs = [ paragraphs[0] ]; 	//remove the parallelism for now
	wait_time = 5 * 1000;
	search_minimum_agreement = .6;
}

var client = null;
if (typeof(soylentJob) == "undefined") {
	if (typeof(paragraphs) == "undefined") {
		paragraphs = [ ["This is the first sentence of the first paragraph."] ]; 
	}

	main();
}



function main() {
	var output = new java.io.FileWriter("fix_errors_results.html");
	var lag_output = new java.io.FileWriter("fix_errors_lag.csv");
	lag_output.write("Stage,Assignment,Wait Type,Time\n");
	var payment_output = new java.io.FileWriter("fix_errors_payment.csv");
	payment_output.write("Stage,Assignment,Cost\n");
	
	var result = {
		paragraphs: []
	}
	for (var paragraph_index = 0; paragraph_index < paragraphs.length; paragraph_index++) {
		attempt(function() {
			print("paragraph #" + paragraph_index);
			print("requesting patches");
			var paragraph = paragraphs[paragraph_index];
			var paragraph_length = getParagraph(paragraph).length;				
			var errors = []
			var error_hit = requestPatches(paragraph);
			print("joining patches");
		
			while (true) {	
				errors = joinPatches(error_hit, paragraph, paragraph_index);
				if (errors.length > 0) {
					break;
				}
				else {
					extendHit(error_hit, buffer_redundancy);
				}	
			}
			cleanUp(error_hit);
			
			output.write(getPaymentString(error_hit, "Error Identification"));	
			output.write(getTimingString(error_hit, "Error Identification"));		

			writeCSVPayment(payment_output, error_hit, "Error Identification");
			writeCSVWait(lag_output, error_hit, "Error Identification");
			
			var finalPatches = {
				sentences: paragraph,
				patches: []
			};
			for (var i=0; i<errors.length; i++) {
				print('Patch #' + i + '\n\n');
				attempt( function() {
					print('\n\n\n');
					var error = errors[i];
					var fix_hit = requestFixes(error);
					
					var reasons = []
					var corrections = []
					while (true) {
						[reasons, corrections] = joinFixes(fix_hit, error.plaintextSentence(), paragraph_index);
						if (reasons.length > 0 && corrections.length > 0) {
							break;
						}
						else {
							extendHit(fix_hit, buffer_redundancy);
						}	
					}
					
					cleanUp(fix_hit);
					var vote_hit = requestVote(error, reasons, corrections, fix_hit, paragraph_index);
					var grammar_votes = []
					var meaning_votes = []


					while (true) {
						[reason_votes, fix_votes] = joinVote(vote_hit, paragraph_index);
						
						if (numVotes(reason_votes) > 0 && numVotes(fix_votes) > 0) {
							break;
						}
						else {
							extendHit(vote_hit, buffer_redundancy);
						}	
					}					
					var outPatch = outputVotes(output, lag_output, payment_output, reason_votes, fix_votes, fix_hit, vote_hit, error, reasons, corrections, paragraph);
					finalPatches.patches.push(outPatch);
					cleanUp(vote_hit);
				});
				print('\n\n\n');
			}
			result.paragraphs.push(finalPatches);
		});
	}
	payment_output.close();
	lag_output.close();
	output.close();
	print(json(result));
	
	var patchesOutput = new java.io.FileWriter("fix_patches.json");	
	patchesOutput.write(json(result));
	patchesOutput.close();	
	
	if (rejectedWorkers.length > 0) {
		print("Rejected workers:");
		print(json(rejectedWorkers.sort()));
	}
}

function requestPatches(paragraph) {
	var text = getParagraph(paragraph);

	var header = read("../../library/hit_header.js").replace(/___BLOCK_WORKERS___/g, [])
				.replace(/___PAGE_NAME___/g, "proofread_identify");

	var webpage = s3.putString(slurp("../template/identify-errors.html")
					.replace(/___PARAGRAPH___/g, text)
					.replace(/___ESCAPED_PARAGRAPH___/g, escape(text))
					.replace(/___HEADER_SCRIPT___/g, header));
	
	// create a HIT on MTurk using the webpage
	var hitId = mturk.createHIT({
		title : "Find bad writing",
		desc : "This paragraph needs some help finding errors. You're way better than Microsoft Word's grammar checker.",
		url : webpage,
		height : 1200,
		assignments: search_redundancy + 2*buffer_redundancy,
		reward : search_reward,
		autoApprovalDelayInSeconds : 60 * 60,
		assignmentDurationInSeconds: 60 * 5		
	})
	return hitId;
}

/*
 * Waits for the shortenable patch HITs, then merges them.
 */
function joinPatches(cut_hit, paragraph, paragraph_index) {
	var status = mturk.getHIT(cut_hit, true)
	print("completed by " + status.assignments.length + " turkers");
	socketStatus(FIND_STAGE, status.assignments.length, paragraph_index);
	
	var hit = mturk.boundedWaitForHIT(cut_hit,wait_time, 1, search_redundancy);
	print("done! completed by " + hit.assignments.length + " turkers");
	
	var patch_suggestions = generatePatchSuggestions(hit.assignments, paragraph);
	//print(json(patch_suggestions));
	var patches = aggregatePatchSuggestions(patch_suggestions, hit.assignments.length, paragraph);
	//print(json(patches));

	print('\n\n\n');

	return patches;
}

var MAX_PATCH_LENGTH = 250;
function generatePatchSuggestions(assignments, paragraph) {
	var suggestions = [];
	var paragraph_length = getParagraph(paragraph).length;
	for (var i=0; i<assignments.length; i++) {
		var user_paragraph = assignments[i].answer.brackets;
		var brackets = /\[\[(.*?)\]\]/g;
		
		var numMatches = 0;
		while((match = brackets.exec(user_paragraph)) != null) {
			var start_index = match.index - (4 * numMatches);	// subtract out [['s
			var end_index = start_index + match[1].length;
			var suggestion = new Patch(start_index, end_index, paragraph);
			var patch_length = end_index - start_index;
			
			if (patch_length > MAX_PATCH_LENGTH || (patch_length >= .90 * paragraph_length && paragraph_length >= 100)) {
				print("WARNING: patch is too long. discarding.");
				// if they just took the whole paragraph, then reject them!				
				print("REJECTING: They highlighted over 90% of the paragraph!");
				rejectedWorkers.push(assignments[i].workerId);
				try {
					mturk.rejectAssignment(assignments[i], "Please, it is not fair to just highlight huge chunks of the paragraph. I am looking for specific areas.");
				} catch(e) {
					print(e);
				}
			} else {			
				var suggestion = new Patch(start_index, end_index, paragraph);
				suggestions.push(suggestion);
			}

			numMatches++;
		}
	}
	suggestions.sort(function(a, b) { return (a.start - b.start); });
	return suggestions;
}

function aggregatePatchSuggestions(patch_suggestions, num_votes, sentences) {
	var open = [];
	var start = null, end = null;
	var patches = [];
	
	var minimum_agreement = Math.max(1, Math.ceil(num_votes * search_minimum_agreement));
	print('number of workers: ' + num_votes);
	print('minimum agreement needed: ' + minimum_agreement + ' overlapping patches');
	
	for (var i=0; i<=getParagraph(sentences).length; i++) {
		for (var j=0; j<patch_suggestions.length; j++) {
			if (i == patch_suggestions[j].start) {
				open.push(patch_suggestions[j]);
				//print(open.length);
				if (open.length == minimum_agreement && start == null) {
					start = open[0].start;
				}
			}

			if (i == patch_suggestions[j].end) {
				open.splice(open.indexOf(open[j]), 1);
				//print(open.length);
				if (open.length == 0 && start != null) {
					end = i;
					patches.push(new Patch(start, end, sentences));
					start = end = null;
				}
			}			
		}
	}
	print(json(patches));
	return patches;
}

function requestFixes(patch) {	
	var highlighted_sentence = 	patch.highlightedParagraph();
	var plaintext = patch.plaintextSentence();

	var header = read("../../library/hit_header.js").replace(/___BLOCK_WORKERS___/g, [])
				.replace(/___PAGE_NAME___/g, "proofread_edit");
	
	
	var webpage = s3.putString(slurp("../template/fix_errors.html")
					.replace(/___HIGHLIGHTED___/g, highlighted_sentence)
					.replace(/___PLAINTEXT___/g, plaintext)
					.replace(/___HEADER_SCRIPT___/g, header));
					
	// create a HIT on MTurk using the webpage
	var hitId = mturk.createHIT({
		title : "Fix Writing Errors",
		desc : "My grammar checker doesn't know what to do with these.",
		url : webpage,
		height : 800,
		assignments: edit_redundancy + buffer_redundancy,
		reward : edit_reward,
		autoApprovalDelayInSeconds: 60 * 60,
		assignmentDurationInSeconds: 60 * 5,
	});
	return hitId;
}

function joinFixes(fix_hit, original_sentence, paragraph_index) {
	print("checking to see if HIT is done")
	var status = mturk.getHIT(fix_hit, true)
	print("completed by " + status.assignments.length + " turkers");
	socketStatus(FIX_STAGE, status.assignments.length, paragraph_index);

	var fix_hit = mturk.boundedWaitForHIT(fix_hit, wait_time, 1, edit_redundancy);

	corrections = []
	reasons = []
	for (var j = 0; j < fix_hit.assignments.length; j++) {
		var result = fix_hit.assignments[j];
		// get the new text from the hit, and display it
		var correction = result.answer.newText;
		print(result.workerId + ": " + correction);
		if (correction == original_sentence) {
			print("REJECTING: They copy/pasted the input.");
			rejectedWorkers.push(result.workerId);
			try {
				mturk.rejectAssignment(result, "Please do not copy/paste the original sentence back in. We're looking for a corrected version.");
			} catch(e) {
				print(e);
			}
			continue;
		}
		var reason = result.answer.description;
		//print(correction);
		if (correction == "" || reason == "") {
			print("REJECTING: They deleted everything, and submitted an empty form.");
			rejectedWorkers.push(e.workerId);
			try {
				mturk.rejectAssignment(e, "You either had an empty correction, or an empty reason for the problem.");
			} catch(e) {
				print(e);
			}			
		}
		else {
			corrections.push(correction);
			reasons.push(reason);
		}
	}
	
	print(json(corrections));
	print(json(reasons));
	return [reasons.unique(), corrections.unique()];
}

function requestVote(patch, reasons, corrections, fix_hit) {
	fix_hit = mturk.getHIT(fix_hit, true);
	var suggestion_workers = []
	for each (var asst in fix_hit.assignments) { 
		if (asst.workerId) suggestion_workers.push(asst.workerId); 
	}
	
	var dmp = new diff_match_patch();

	// provide a challenge if there is only one option
	if (corrections.length == 1) {
		var original = patch.plaintextSentence();
		if (original != corrections[0]) {
			corrections.push(original);
		}
	}
	
	var t_reason = '<table>';
	foreach(reasons, function (reason, j) {		
		var reason_row = '<tr valign="top" class="reason"><td><label><input type="checkbox" name="reason" value="' + escape(reason) + '"></input></label></td><td>' + reason + '</td></tr>';
		t_reason += reason_row;
	});
	t_reason += '</table>';

	var t_fix = '<table>';	
	foreach(corrections, function (correction, j) {
		var diff = dmp.diff_main(patch.plaintextSentence(), correction);
		dmp.diff_cleanupSemantic(diff);		
		var diff_html = "<div>" + dmp.diff_prettyHtml(diff) + "</div>";		
				
		var fix_row = '<tr valign="top" class="fix"><td><label><input type="checkbox" name="fix" value="' + escape(correction) + '"></input></td><td>' +  diff_html + '</td></tr>';
		t_fix += fix_row;
	});
	t_fix += '</table>';
	
	var header = read("../../library/hit_header.js").replace(/___BLOCK_WORKERS___/g, suggestion_workers)
					.replace(/___PAGE_NAME___/g, "grammar_vote");
	
	var w = s3.putString(slurp("../template/vote_errors.html")
		.replace(/___REASON_TABLE___/g, t_reason)
		.replace(/___FIX_TABLE___/g, t_fix)		
		.replace(/___HIGHLIGHTED___/g, patch.highlightedSentence())
		.replace(/___HEADER_SCRIPT___/g, header));
	
	// create a HIT on MTurk using the webpage
	var hitId = mturk.createHIT({
		title : "Vote on writing suggestions",
		desc : "Which sentence improvement should I use?",
		url : w,
		height : 800,
		assignments: verify_redundancy + buffer_redundancy,
		reward : verify_reward,
		autoApprovalDelayInSeconds: 60 * 60,
		assignmentDurationInSeconds: 60 * 5,
	});
	return hitId;
}

function joinVote(vote_hit, paragraph_index) {
	print("checking to see if HIT is done");
	print(vote_hit);
	var status = mturk.getHIT(vote_hit, true)
	print("completed by " + status.assignments.length + " turkers");
	socketStatus(FILTER_STAGE, status.assignments.length, paragraph_index);
	
	
	var vote_hit = mturk.boundedWaitForHIT(vote_hit, wait_time, 1, verify_redundancy);
	foreach(vote_hit.assignments, function(assignment) {
		if (typeof(assignment.answer.reason) == "undefined" || typeof(assignment.answer.fix) == "undefined") {
			print("REJECTING: No data.");
			rejectedWorkers.push(assignment.workerId);
			try {
				mturk.rejectAssignment(assignment, "You seem to have submitted an empty form.");
			} catch(e) {
				print(e);
			}			
		}
	});	
		
	var reason_votes = get_vote(vote_hit.assignments, function(answer) { 
		if (typeof(answer.reason) == "undefined") return [];
		
		var results = [];
		foreach(answer.reason.split('|'), function(checked, i) {
			results.push(unescape(checked));
		});
		return results;
	}, true);
	var fix_votes = get_vote(vote_hit.assignments, function(answer) {
		if (typeof(answer.fix) == "undefined") return [];
		var results = [];
		foreach(answer.fix.split('|'), function(checked, i) {
			results.push(unescape(checked));
		});
		return results;		
	}, true);
	print(json(reason_votes));
	print(json(fix_votes));
	return [reason_votes, fix_votes];
}

function outputVotes(output, lag_output, payment_output, reason_votes, fix_votes, edit_hit, vote_hit, patch, reasons, corrections, paragraph) {
	output.write(preWrap(getParagraph(paragraph)));
	
	var edit_hit = mturk.getHIT(edit_hit, true)	
	var vote_hit = mturk.getHIT(vote_hit, true)	
	
	output.write("<h1>Patch</h1>");
	output.write("<h2>Original</h2>" + preWrap(patch.highlightedSentence()));	
	

	output.write(getPaymentString(edit_hit, "Fixing Error"));
	output.write(getTimingString(edit_hit, "Fixing Error"));
	
	output.write(getPaymentString(vote_hit, "Voting on Alternatives"));
	output.write(getTimingString(edit_hit, "Voting on Alternatives"));
	
	var patchOutput = {
		start: patch.start,
		end: patch.end,
		options: [],
		reasons: []
	}
	patchOutput.numVotes = vote_hit.assignments.length;	
	
	var dmp = new diff_match_patch();	
	output.write("<h3>Correction Votes</h3>");
	for (var i=0; i<corrections.length; i++) {
		var correction = corrections[i];
		var diff = dmp.diff_main(patch.plaintextSentence(), correction);
		dmp.diff_cleanupSemantic(diff);		
		var diff_html = dmp.diff_prettyHtml(diff);
		
		var votes = fix_votes[correction] ? fix_votes[correction] : 0;
		output.write("<div>" + diff_html + ": " + votes + "</div>");
		
		var passesVote = (votes / vote_hit.assignments.length) >= .3;
		if (passesVote) {
			patchOutput.options.push(correction);
		}
	}
	
	output.write("<h3>Reason Votes</h3>");
	for (var i = 0; i < reasons.length; i++) {
		var reason = reasons[i];
		var votes = reason_votes[reason] ? reason_votes[reason] : 0;
		output.write("<div>" + reason + ": " + votes + "</div>");
		
		var passesVote = (votes / vote_hit.assignments.length) >= .3;
		if (passesVote) {
			patchOutput.reasons.push(reason);
		}		
	}	
	
	writeCSVPayment(payment_output, edit_hit, "Fixing Error");
	writeCSVPayment(payment_output, vote_hit, "Voting on Alternatives");
	
	writeCSVWait(lag_output, edit_hit, "Fixing Error");
	writeCSVWait(lag_output, vote_hit, "Voting on Alternatives");
	output.flush();
	payment_output.flush();
	lag_output.flush();
	return patchOutput;
}
