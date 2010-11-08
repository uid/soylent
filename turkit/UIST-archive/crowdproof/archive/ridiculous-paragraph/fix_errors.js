// imports
eval(read("../library/patch.js"));
eval(read("../library/hit_utils.js"));
eval(read("../library/diff_match_patch_uncompressed.js"));

var search_reward = 0.06;
var search_redundancy = 10;
var search_minimum_agreement = 0.20

var edit_reward = 0.08;
var edit_redundancy = 5;

var verify_redundancy = 5;
var verify_reward = 0.03;

var paragraphs = [ ["Marketing are bad for brand big and small.", "You Know What I am Saying.", "It is no wondering that advertisings are bad for company in America, Chicago and Germany.", "Updating of brand image are bad for processes in one company and many companies."] ];

var debug = false;

if (debug)
{
	search_redundancy = 1;
	edit_redundancy = 1;
	verify_redundancy = 1;
	paragraphs = [ paragraphs[0] ]; 	//remove the parallelism for now
}

main();



function main() {
	var output = new java.io.FileWriter("fix_errors_results.html");
	var lag_output = new java.io.FileWriter("fix_errors_lag.csv");
	lag_output.write("Stage,Assignment,Wait Type,Time\n");
	var payment_output = new java.io.FileWriter("fix_errors_payment.csv");
	payment_output.write("Stage,Assignment,Cost\n");
	
	for (var paragraph_index = 0; paragraph_index < paragraphs.length; paragraph_index++) {
		attempt(function() {
			print("paragraph #" + paragraph_index);
			print("requesting patches");
			var paragraph = paragraphs[paragraph_index];
			var error_hit = requestPatches(paragraph);
			print("joining patches");
			errors = joinPatches(error_hit, paragraph);
			cleanUp(error_hit);
			
			output.write(getPaymentString(error_hit, "Error Identification"));	
			output.write(getTimingString(error_hit, "Error Identification"));		

			writeCSVPayment(payment_output, error_hit, "Error Identification");
			writeCSVWait(lag_output, error_hit, "Error Identification");
			
			for (var i=0; i<errors.length; i++) {
				attempt( function() {
					print('\n\n\n');
					var error = errors[i];
					var fix_hit = requestFixes(error);
					[reasons, corrections] = joinFixes(fix_hit);
					var vote_hit = requestVote(error, reasons, corrections, fix_hit);
					[reason_votes, fix_votes] = joinVote(vote_hit);
					outputVotes(output, lag_output, payment_output, reason_votes, fix_votes, fix_hit, vote_hit, error, reasons, corrections, paragraph);
					cleanUp(fix_hit);
					cleanUp(vote_hit);
				});
				print('\n\n\n');
			}
		});
	}
	payment_output.close();
	lag_output.close();
	output.close();
}

function requestPatches(paragraph) {
	var text = getParagraph(paragraph);

	var header = read("../library/hit_header.js").replace(/___BLOCK_WORKERS___/g, [])
				.replace(/___PAGE_NAME___/g, "proofread_identify");

	var webpage = s3.putString(slurp("identify-errors.html")
					.replace(/___PARAGRAPH___/g, text)
					.replace(/___ESCAPED_PARAGRAPH___/g, escape(text))
					.replace(/___HEADER_SCRIPT___/g, header));
	
	// create a HIT on MTurk using the webpage
	var hitId = mturk.createHIT({
		title : "Find bad writing",
		desc : "This paragraph needs some help finding errors. You're way better than Microsoft Word's grammar checker.",
		url : webpage,
		height : 1200,
		assignments: search_redundancy,
		reward : search_reward,
		autoApprovalDelayInSeconds : 60,
		assignmentDurationInSeconds: 60 * 5		
	})
	return hitId;
}

/*
 * Waits for the shortenable patch HITs, then merges them.
 */
function joinPatches(cut_hit, paragraph) {
	var status = mturk.getHIT(cut_hit, true)
	print("completed by " + status.assignments.length + " turkers");
	var hit = mturk.waitForHIT(cut_hit)
	print("done! completed by " + hit.assignments.length + " turkers");
	
	var patch_suggestions = generatePatchSuggestions(hit.assignments, paragraph);
	//print(json(patch_suggestions));
	var patches = aggregatePatchSuggestions(patch_suggestions, hit.assignments.length, paragraph);
	//print(json(patches));

	print('\n\n\n');

	return patches;
}

function generatePatchSuggestions(assignments, paragraph) {
	var suggestions = [];
	for (var i=0; i<assignments.length; i++) {
		var user_paragraph = assignments[i].answer.brackets;
		var brackets = /\[\[(.*?)\]\]/g;
		
		var numMatches = 0;
		while((match = brackets.exec(user_paragraph)) != null) {
			var start_index = match.index - (4 * numMatches);	// subtract out [['s
			var end_index = start_index + match[1].length;
			var suggestion = new Patch(start_index, end_index, paragraph);
			suggestions.push(suggestion);
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
	
	var minimum_agreement = Math.max(1, Math.floor(num_votes * search_minimum_agreement));
	print('number of workers: ' + num_votes);
	print('minimum agreement needed: ' + minimum_agreement + ' overlapping patches');
	
	for (var i=0; i<getParagraph(sentences).length; i++) {
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

	var header = read("../library/hit_header.js").replace(/___BLOCK_WORKERS___/g, [])
				.replace(/___PAGE_NAME___/g, "proofread_edit");
	
	
	var webpage = s3.putString(slurp("fix_errors.html")
					.replace(/___HIGHLIGHTED___/g, highlighted_sentence)
					.replace(/___PLAINTEXT___/g, plaintext)
					.replace(/___HEADER_SCRIPT___/g, header));

	// create a HIT on MTurk using the webpage
	var hitId = mturk.createHIT({
		title : "Fix Writing Errors",
		desc : "My grammar checker doesn't know what to do with these.",
		url : webpage,
		height : 800,
		assignments: edit_redundancy,
		reward : edit_reward,
		autoApprovalDelayInSeconds: 60,
		assignmentDurationInSeconds: 60 * 5,
	});
	return hitId;
}

function joinFixes(fix_hit) {
	print("checking to see if HIT is done")
	var status = mturk.getHIT(fix_hit, true)
	print("completed by " + status.assignments.length + " turkers");
	var fix_hit = mturk.waitForHIT(status);

	corrections = []
	reasons = []
	for (var j = 0; j < fix_hit.assignments.length; j++) {
		var result = fix_hit.assignments[j];
		// get the new text from the hit, and display it
		var correction = result.answer.newText;
		var reason = result.answer.description;
		//print(correction);
		if (correction != "") {
			corrections.push(correction);
		}
		if (reason != "") {	
			reasons.push(reason);
		}
	}
	
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
	
	var header = read("../library/hit_header.js").replace(/___BLOCK_WORKERS___/g, suggestion_workers)
					.replace(/___PAGE_NAME___/g, "grammar_vote");
	
	var w = s3.putString(slurp("vote_errors.html")
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
		assignments: verify_redundancy,
		reward : verify_reward,
		autoApprovalDelayInSeconds: 60,
		assignmentDurationInSeconds: 60 * 5,
	});
	return hitId;
}

function joinVote(vote_hit) {
	print("checking to see if HIT is done");
	print(vote_hit);
	var status = mturk.getHIT(vote_hit, true)
	print("completed by " + status.assignments.length + " turkers");
	var vote_hit = mturk.waitForHIT(vote_hit);
	
	var reason_votes = get_vote(vote_hit.assignments, function(answer) { 
		var results = [];
		foreach(answer.reason.split('|'), function(checked, i) {
			results.push(unescape(checked));
		});
		return results;
	}, true);
	var fix_votes = get_vote(vote_hit.assignments, function(answer) {
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
	
	output.write("<h1>Patch</h1>");
	output.write("<h2>Original</h2>" + preWrap(patch.highlightedSentence()));	
	

	output.write(getPaymentString(edit_hit, "Fixing Error"));
	output.write(getTimingString(edit_hit, "Fixing Error"));
	
	output.write(getPaymentString(vote_hit, "Voting on Alternatives"));
	output.write(getTimingString(edit_hit, "Voting on Alternatives"));
	
	var dmp = new diff_match_patch();	
	output.write("<h3>Correction Votes</h3>");
	for (var i=0; i<corrections.length; i++) {
		var correction = corrections[i];
		var diff = dmp.diff_main(patch.plaintextSentence(), correction);
		dmp.diff_cleanupSemantic(diff);		
		var diff_html = dmp.diff_prettyHtml(diff);		
		
		var votes = fix_votes[correction] ? fix_votes[correction] : 0;
		output.write("<div>" + diff_html + ": " + votes + "</div>");
	}
	
	output.write("<h3>Reason Votes</h3>");
	for (var i = 0; i < reasons.length; i++) {
		var reason = reasons[i];
		var votes = reason_votes[reason] ? reason_votes[reason] : 0;
		output.write("<div>" + reason + ": " + votes + "</div>");
	}	
	
	writeCSVPayment(payment_output, edit_hit, "Fixing Error");
	writeCSVPayment(payment_output, vote_hit, "Voting on Alternatives");
	
	writeCSVWait(lag_output, edit_hit, "Fixing Error");
	writeCSVWait(lag_output, vote_hit, "Voting on Alternatives");
	output.flush();
	payment_output.flush();
	lag_output.flush();
}
