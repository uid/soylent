// imports
eval(read("../library/patch.js"));
eval(read("../library/hit_utils.js"));
eval(read("../library/diff_match_patch_uncompressed.js"));

var buffer_redundancy = 2;	// number of extra assignments to create so that they don't get squatted.
var wait_time = 20 * 60 * 1000;	// seconds

var search_reward = 0.08;
var search_redundancy = 10;
var search_minimum_agreement = 0.20

var edit_reward = 0.05;
var edit_redundancy = 5;  // number of turkers requested for each HIT

var verify_reward = 0.04;
var verify_redundancy = 5;

var time_bounded = true;
var rejectedWorkers = []

var find = true;
var fix = true;
var filter = true;
var debug = false;

var client = null;
if (typeof(soylentJob) == "undefined") {
	if (typeof(paragraphs) == "undefined") {
		paragraphs = [ ["This is the first sentence of the first paragraph."] ]; 
	}

	main();
}

// main program
function main() {
	var output = new java.io.FileWriter("active_hits\shortn-results." + soylentJob + ".html");
	var lag_output = new java.io.FileWriter("active-hits\shortn-" + soylentJob + "-fix_errors_lag.csv");
	lag_output.write("Stage,Assignment,Wait Type,Time,Paragraph\n");
	var payment_output = new java.io.FileWriter("active-hits\shortn-" + soylentJob + "-fix_errors_payment.csv");
	payment_output.write("Stage,Assignment,Cost,Paragraph\n");
	var patchesOutput = new java.io.FileWriter("active_hits\shortn-patches." + soylentJob +".json");	
	
	if (debug)
	{
		print('debug version');
		search_redundancy = 1;
		edit_redundancy = 1;
		verify_redundancy = 1;
		paragraphs = [ paragraphs[0] ]; 	//remove the parallelism for now
		wait_time = 5 * 1000;
		search_minimum_agreement = .6;
	}	
	
	var result = {
		paragraphs: []
	}
	
	var overallFastestParagraph = Number.MAX_VALUE;
	var overallSlowestParagraph = Number.MIN_VALUE;	
	
	for (var paragraph_index = 0; paragraph_index < paragraphs.length; paragraph_index++) {
		if (typeof(cuts) != "undefined") {
			var providedCuts = cuts;
		}
		attempt(function() {
			print('\n\n\n');		
			print("paragraph #" + paragraph_index);
			var paragraph = paragraphs[paragraph_index];
			
			var findTime;
			var maxFixVerifyTime = Number.MIN_VALUE;
			var minFixVerifyTime = Number.MAX_VALUE;
			var findWorkTime;
			
			if (find) {
				print("requesting patches");
				var cuts = [];
				var cut_hit = requestPatches(paragraph);
				
				print("joining patches");

				while (true) {	
					cuts = joinPatches(cut_hit, paragraph, paragraph_index);	
					if (cuts.length > 0) {
						break;
					}
					else {
						extendHit(cut_hit, buffer_redundancy);
					}	
				}
				cleanUp(cut_hit);
				
				output.write(getPaymentString(cut_hit, "Find"));	
				output.write(getTimingString(cut_hit, "Find"));

				writeCSVPayment(payment_output, cut_hit, "Find", paragraph_index);
				writeCSVWait(lag_output, cut_hit, "Find", paragraph_index);
			}
			
			var finalPatches = {
				sentences: paragraph,
				patches: []
			};
			
			if (typeof(cuts) == "undefined" && typeof(providedCuts) != "undefined") {
				var cuts = providedCuts;
			}
			
			findTime = getHITEndTime(cut_hit) - getHITStartTime(cut_hit);
			print("Find time: " + findTime);
			
			for (var i=0; i<cuts.length; i++) {
				print('Patch #' + i + '\n\n');
				var cut = cuts[i];
				attempt(function() {
					if (fix) {
						print("requesting edits");
						var edit_hit = requestEdits(cut);					
						print("joining edits");
						
						var suggestions = []
						while (true) {	
							suggestions = joinEdits(edit_hit, cut.plaintextSentence(), paragraph_index);
							if (suggestions.length > 0) {
								break;
							}
							else {
								extendHit(edit_hit, buffer_redundancy);
							}	
						}		
						
						cleanUp(edit_hit);
					}
					
					if (filter) {
						print("requesting votes");					
						var vote_hit = requestVotes(cut, suggestions, edit_hit);
						
						print("joining votes");					
						var grammar_votes = [];
						var meaning_votes = [];
						while (true) {
							[grammar_votes, meaning_votes] = joinVotes(vote_hit, paragraph_index);
							
							if (numVotes(grammar_votes) > 0 && numVotes(meaning_votes) > 0) {
								break;
							}
							else {
								extendHit(vote_hit, buffer_redundancy);
							}	
						}				
						
						cleanUp(vote_hit);
					}
					
					var fixTime = getHITEndTime(edit_hit) - getHITStartTime(edit_hit);
					var verifyTime = getHITEndTime(vote_hit) - getHITStartTime(vote_hit);
					print('fix time: ' + fixTime);
					print('verify time: ' + verifyTime);
					maxFixVerifyTime = Math.max(fixTime+verifyTime, maxFixVerifyTime);
					minFixVerifyTime = Math.min(fixTime+verifyTime, minFixVerifyTime);
					
					var outPatch = outputEdits(output, lag_output, payment_output, paragraph, cut, cut_hit, edit_hit, vote_hit, grammar_votes, meaning_votes, suggestions, paragraph_index);
					finalPatches.patches.push(outPatch);
				});
				print('\n\n\n');
			}
			
			finalPatches.patches.sort( function(a, b) { return a.length - b.length; } );
			result.paragraphs.push(finalPatches);
			
			print('Find: ' + findTime);
			print('Longest Fix+Verify: ' + maxFixVerifyTime);
			print('Shortest Fix+Verify: ' + minFixVerifyTime);
			print('Max Elapsed time (seconds): ' + (findTime + maxFixVerifyTime) / 1000);
			print('Max Elapsed time (minutes): ' + ((findTime + maxFixVerifyTime) / (1000*60)));
			print('Min Elapsed time (seconds): ' + (findTime + minFixVerifyTime) / 1000);
			print('Min Elapsed time (minutes): ' + ((findTime + minFixVerifyTime) / (1000*60)));			
			overallFastestParagraph = Math.min(overallFastestParagraph, (findTime + maxFixVerifyTime));
			overallSlowestParagraph = Math.max(overallSlowestParagraph, (findTime + maxFixVerifyTime));			
		});	
	}
	payment_output.close();
	lag_output.close();	
	output.close();

	patchesOutput.write(json(result));
	patchesOutput.close();
	
	print(json(result));
	
	if (rejectedWorkers.length > 0) {
		print("Rejected workers:");
		print(json(rejectedWorkers.sort()));
	}
	
	print('Fastest paragraph (minutes): ' + overallFastestParagraph / (1000*60));
	print('Slowest paragraph (minutes): ' + overallSlowestParagraph / (1000*60));	
}

/**
 * Creates HITs to find shortenable regions in the paragraph
 */
function requestPatches(paragraph) {
	var text = getParagraph(paragraph);

	var webpage = s3.putString(slurp("../templates/shortn/shortn-find.html").replace(/___PARAGRAPH___/g, text).replace(/___ESCAPED_PARAGRAPH___/g, escape(text)));
	//webpage = webpage.replace(new RegExp('^http://'), 'https://')	// avoid IE errors
	//print(webpage);
	
	// create a HIT on MTurk using the webpage
	var hitId = mturk.createHIT({
		title : "Find unnecessary text",
		desc : "I need to shorten my paragraph, and need opinions on what to cut.",
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
    print(json(status));
	socketStatus(FIND_STAGE, status.assignments.length, paragraph_index);
	
	var hit = mturk.boundedWaitForHIT(cut_hit, wait_time, 6, search_redundancy);
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
					//print('opening');
					start = open[0].start;
				}
			}

			if (i == patch_suggestions[j].end) {
				open.splice(open.indexOf(open[j]), 1);
				//print(open.length);
				if (open.length == 0 && start != null) {
					//print('closing');
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

/*
 * Takes in patches of cuttable areas and spawns HITs to gather alternatives.
 */
function requestEdits(cut) {	
	var full_text = cut.highlightedParagraph();
	var editable = cut.plaintextSentence();

	var webpage = s3.putString(slurp("../templates/shortn/shortn-fix.html").replace(/___TEXT___/g, full_text)
					.replace(/___EDITABLE___/g, editable));
	

	// create a HIT on MTurk using the webpage
	var edit_hit = mturk.createHIT({
		title : "Shorten Rambling Text",
		desc : "A sentence in my paper is too long and I need your help cutting out the fat.",
		url : webpage,
		height : 800,
		assignments: edit_redundancy + buffer_redundancy,
		reward : edit_reward,
		autoApprovalDelayInSeconds : 60 * 60,
		assignmentDurationInSeconds: 60 * 5
	})
	return edit_hit;
}

/*
 * Waits for all the edits to be completed
 * @return: all the unique strings that turkers suggested
 */
function joinEdits(edit_hit, originalSentence, paragraph_index) {
	var hitId = edit_hit;
	print("checking to see if HIT is done")
	var status = mturk.getHIT(hitId, true)	
	print("completed by " + status.assignments.length + " turkers");
	socketStatus(FIX_STAGE, status.assignments.length, paragraph_index);
	
	var hit = mturk.boundedWaitForHIT(hitId, wait_time, 3, edit_redundancy);
	print("done! completed by " + hit.assignments.length + " turkers");
	
	var options = new Array();
	foreach(hit.assignments, function(e) {
		var answer = e.answer.newText;
		if (answer == originalSentence) {
			print("REJECTING: They copy/pasted the input.");
			rejectedWorkers.push(e.workerId);
			try {
				mturk.rejectAssignment(e, "Please do not copy/paste the original sentence back in. We're looking for a shorter version.");
			} catch(e) {
				print(e);
			}
		}
		else if (answer.length >= originalSentence) {
			print("REJECTING: They made the sentence longer.");
			rejectedWorkers.push(e.workerId);
			try {
				mturk.rejectAssignment(e, "Your sentence was as long or longer than the original. We're looking for a shorter version.");
			} catch(e) {
				print(e);
			}		
		}
		else {
			options.push(e.answer.newText) 
		}
	});
	var unique_options = options.unique();	
	return unique_options;
}

/*
 * Requests a vote filter for the options based on grammaticality and changing meaning
 */
function requestVotes(cut, options, edit_hit) {		
	// Disallow workers from the edit hits from working on the voting hits
	edit_workers = []
	for each (var asst in edit_hit.assignments) { 
		if (asst.workerId) edit_workers.push(asst.workerId); 
	}
	
	var dmp = new diff_match_patch();
	
	// provide a challenge if there is only one option
	if (options.length == 1) {
		var original = cut.plaintextSentence();
		if (original != options[0]) {
			options.push(original);
		}
	}	

	var t_grammar = '<table>';
	var t_meaning = '<table>';	
	foreach(options, function (correction, j) {
		var diff = dmp.diff_main(cut.plaintextSentence(), correction);
		dmp.diff_cleanupSemantic(diff);		
		var diff_html = "<div>" + dmp.diff_prettyHtml(diff) + "</div>";		
		
		var grammar_row = '<tr valign="top" class="grammar"><td><label><input type="checkbox" name="grammar" value="' + escape(correction) + '"></input></label></td><td>' +  diff_html + '</td></tr>';
		t_grammar += grammar_row;
		
		var meaning_row = '<tr valign="top" class="meaning"><td><label><input type="checkbox" name="meaning" value="' + escape(correction) + '"></input></td><td>' +  diff_html + '</td></tr>';
		t_meaning += meaning_row;
	});
	t_grammar += '</table>';
	t_meaning += '</table>';
	
	// Now we create a hit to vote on whether it's good
	var header = read("../library/hit_header.js").replace(/___BLOCK_WORKERS___/g, edit_workers)
					.replace(/___PAGE_NAME___/g, "shorten_vote");
					
	var webpage = s3.putString(slurp("../templates/shortn/shortn-verify.html")
		.replace(/___HIGHLIGHTED___/g, cut.highlightedParagraph())	
		.replace(/___GRAMMAR_VOTE___/g, t_grammar)
		.replace(/___MEANING_VOTE___/g, t_meaning)		
		.replace(/___HEADER_SCRIPT___/g, header));					
	
	// create a HIT on MTurk using the webpage
	var vote_hit = mturk.createHIT({
		title : "Did I shorten text correctly?",
		desc : "I need to shorten some text -- which version is best?",
		url : webpage,
		height : 800,
		assignments: verify_redundancy + buffer_redundancy, 
		reward : verify_reward,
		autoApprovalDelayInSeconds : 60 * 60,
		assignmentDurationInSeconds: 60 * 5
	})
	return vote_hit;
}

function joinVotes(vote_hit, paragraph_index) {
	// get the votes
	var hitId = vote_hit;
	var status = mturk.getHIT(hitId, true)	
	print("completed by " + status.assignments.length + " turkers");
	socketStatus(FILTER_STAGE, status.assignments.length, paragraph_index);
	
	var hit = mturk.boundedWaitForHIT(hitId, wait_time, 3, verify_redundancy);
	print("done! completed by " + hit.assignments.length + " turkers");
	
	foreach(hit.assignments, function(assignment) {
		if (typeof(assignment.answer.grammar) == "undefined" || typeof(assignment.answer.meaning) == "undefined") {
			print("REJECTING: No data.");
			rejectedWorkers.push(assignment.workerId);
			try {
				mturk.rejectAssignment(assignment, "You seem to have submitted an empty form.");
			} catch(e) {
				print(e);
			}			
		}
	});
	
	var grammar_votes = get_vote(hit.assignments, function(answer) { 
		if (typeof(answer.grammar) == "undefined") return [];
		
		var results = [];
		foreach(answer.grammar.split('|'), function(checked, i) {
			results.push(unescape(checked));
		});
		return results;
	}, true);
	var meaning_votes = get_vote(hit.assignments, function(answer) {
		if (typeof(answer.meaning) == "undefined") return [];
	
		var results = [];
		foreach(answer.meaning.split('|'), function(checked, i) {
			results.push(unescape(checked));
		});
		return results;		
	}, true);
	
	print('grammar');
	print(json(grammar_votes));
	print('meaning');
	print(json(meaning_votes));
	
	return [grammar_votes, meaning_votes];
}

function outputEdits(output, lag_output, payment_output, paragraph, cut, cut_hit, edit_hit, vote_hit, grammar_votes, meaning_votes, suggestions, paragraph_index)
{	
	output.write(preWrap(getParagraph(paragraph)));

	if (cut_hit != null) {
		var cut_hit = mturk.getHIT(cut_hit, true);
	}
	else {
		print("OUTPUTTING NO FIND HIT");
	}
	
	if (edit_hit != null) {
		var edit_hit = mturk.getHIT(edit_hit, true)	
		output.write(getPaymentString(edit_hit, "Shortened Version Editing"));	
		output.write(getTimingString(edit_hit, "Shortened Version Editing"));	
		output.write(getPaymentString(edit_hit, "Fix"));
		output.write(getTimingString(edit_hit, "Fix"));

		writeCSVPayment(payment_output, edit_hit, "Fixing Error", paragraph_index);
		writeCSVWait(lag_output, edit_hit, "Fixing Error", paragraph_index);		
	}
	else {
		print("OUTPUTTING NO FIX HIT");
	}
	
	if (vote_hit != null) {
		var vote_hit = mturk.getHIT(vote_hit, true);
		output.write(getPaymentString(vote_hit, "Voting"));	
		output.write(getTimingString(vote_hit, "Voting"));				
		output.write(getPaymentString(vote_hit, "Vote"));
		output.write(getTimingString(edit_hit, "Vote"));

		writeCSVPayment(payment_output, vote_hit, "Voting on Alternatives", paragraph_index);
		writeCSVWait(lag_output, vote_hit, "Voting on Alternatives", paragraph_index);		
	}
	else {
		print("OUTPUTTING NO FILTER HIT");
	}
	
	output.write("<h1>Patch</h1>");
	output.write("<h2>Original</h2>" + preWrap(cut.highlightedSentence()));
	
	var patch = {
		start: cut.start,
		end: cut.end,
		options: [],
		paragraph: paragraph
	}
	
	if (edit_hit != null) {
		cuttable_votes = get_vote(edit_hit.assignments, (function(answer) { return answer.cuttable; }));
		var numSayingCuttable = cuttable_votes['Yes'] ? cuttable_votes['Yes'] : 0;
		output.write("<p>Is it cuttable?  <b>" + numSayingCuttable + "</b> of " + edit_hit.assignments.length + " turkers say yes.</p>");
		
		patch.canCut = ((numSayingCuttable / edit_hit.assignments.length) > .5);
		patch.cutVotes = numSayingCuttable;
		patch.numEditors = edit_hit.assignments.length;
	}
	
	var dmp = new diff_match_patch();
	
	if (suggestions != null) {
		for (var i = 0; i < suggestions.length; i++) {
			// this will be one of the alternatives they generated
			//var result = edit_hit.assignments[i]
			//var newText = result.answer.newText
			var newText = suggestions[i];
			
			var this_grammar_votes = grammar_votes[newText] ? grammar_votes[newText] : 0;
			var this_meaning_votes = meaning_votes[newText] ? meaning_votes[newText] : 0;

			var diff = dmp.diff_main(cut.plaintextSentence(), newText);
			dmp.diff_cleanupSemantic(diff);		
			var diff_html = "<div>" + dmp.diff_prettyHtml(diff) + "</div>";		
			
			output.write(diff_html);
			output.write("<div>How many people thought this had the most grammar problems? <b>" + this_grammar_votes + "</b> of " + vote_hit.assignments.length + " turkers.</div>");
			output.write("<div>How many people thought this changed the meaning most? <b>" + this_meaning_votes + "</b> of " + vote_hit.assignments.length + " turkers.</div>");		
			output.flush();
			
			patch.numVoters = vote_hit.assignments.length;
			
			var passesGrammar = (this_grammar_votes / vote_hit.assignments.length) < .5;
			var passesMeaning = (this_meaning_votes / vote_hit.assignments.length) < .5;
			if (passesGrammar && passesMeaning) {
				patch.options.push(newText);
			}
		}
		
		print("TODO: send entire paragraph option rather than patch-by-patch (whoops)");
		socketShorten(patch);
	}
	
	return patch;
}