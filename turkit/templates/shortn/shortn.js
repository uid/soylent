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

var findStageOn = true;
var fixStageOn = true;
var verifyStageOn = true;

var output;
var lag_output;
var payment_output;
var patchesOutput;

var overallFastestParagraph = Number.MAX_VALUE;
var overallSlowestParagraph = Number.MIN_VALUE;	

var fileOutputOn = false;

var client = null;
if (typeof(soylentJob) == "undefined") {
	if (typeof(paragraphs) == "undefined") {
		paragraphs = [ ["This is the first sentence of the first paragraph."] ]; 
	}

	main();
}
if (typeof(debug) == "undefined") {
    var debug = false;
}

// main program
function main() {
    initializeOutput();	
    initializeDebug();
	
	var result = {
		paragraphs: []
	}
	
	for (var paragraph_index = 0; paragraph_index < paragraphs.length; paragraph_index++) {
		if (typeof(cuts) != "undefined") {
			var providedCuts = cuts;
		}
		attempt(function() {
			print('\n\n\n');		
			print("paragraph #" + paragraph_index);
			var paragraph = paragraphs[paragraph_index];
            var maxFixVerifyTime = Number.MIN_VALUE;
			var minFixVerifyTime = Number.MAX_VALUE;
            
            [cuts, cut_hit] = findPatches(paragraph, paragraph_index, output, payment_output, lag_output);
            if (typeof(cuts) == "undefined" && typeof(providedCuts) != "undefined") {
				var cuts = providedCuts;
			}
            var findTime = getHITEndTime(cut_hit) - getHITStartTime(cut_hit);
			print("Find time: " + findTime);            

			var patches = {
				paragraph: paragraph_index,
				patches: []
			};
			
            var finishedArray = new Array();
            for (var i=0; i<cuts.length; i++) {
                finishedArray[i] = false;
            }
            
			for (var i=0; i<cuts.length; i++) {
				print('Patch #' + i + '\n\n');
				var cut = cuts[i];
				attempt(function() {
                    [suggestions, edit_hit] = fixPatches(cut, paragraph_index);
                    [grammar_votes, meaning_votes, vote_hit] = verifyPatches(cut, edit_hit, suggestions, paragraph_index);
                    
					var fixTime = getHITEndTime(edit_hit) - getHITStartTime(edit_hit);
					var verifyTime = getHITEndTime(vote_hit) - getHITStartTime(vote_hit);
					print('fix time: ' + fixTime);
					print('verify time: ' + verifyTime);
					maxFixVerifyTime = Math.max(fixTime+verifyTime, maxFixVerifyTime);
					minFixVerifyTime = Math.min(fixTime+verifyTime, minFixVerifyTime);
					
                    var patch = generatePatch(cut, cut_hit, edit_hit, vote_hit, grammar_votes, meaning_votes, suggestions, paragraph_index);
                    print('new patch yay')
                    print(json(patch))
                    patches.patches.push(patch);
                    
					outputEdits(output, lag_output, payment_output, paragraph, cut, cut_hit, edit_hit, vote_hit, grammar_votes, meaning_votes, suggestions, paragraph_index, patch);
                    finishedArray[i] = true;
				} );
				print('\n\n\n');
			}
			
            if (!finishedPatches(finishedArray)) {
                // wait if not all the patches for the paragraph are complete
                stop();
            }
            
            // Now we merge patch revision bounds to see if there is any overlap between edits in various patches.
            // If so, we merge the patches together.
            patches.patches = mergePatches(patches.patches, paragraph_index);
            
			patches.patches.sort( function(a, b) { return a.start - b.start; } );
            socketShortn(patches);
            
			result.paragraphs.push(patches);
			
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
    
    if (fileOutputOn) {    
        payment_output.close();
        lag_output.close();	
        output.close();

        patchesOutput.write(json(result));
        patchesOutput.close();
    }
	
	print(json(result));
	
	if (rejectedWorkers.length > 0) {
		print("Rejected workers:");
		print(json(rejectedWorkers.sort()));
	}
	
	print('Fastest paragraph (minutes): ' + overallFastestParagraph / (1000*60));
	print('Slowest paragraph (minutes): ' + overallSlowestParagraph / (1000*60));	
}

/**
 * Finds shortenable patches
 */
function findPatches(paragraph, paragraph_index, output, payment_output, lag_output) {
    if (findStageOn) {
        print("requesting patches");
        var cut_hit = requestPatches(paragraph);
        
        print("joining patches");
        var cuts = [];

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
        
        if (fileOutputOn) {
            output.write(getPaymentString(cut_hit, "Find"));	
            output.write(getTimingString(cut_hit, "Find"));

            writeCSVPayment(payment_output, cut_hit, "Find", paragraph_index);
            writeCSVWait(lag_output, cut_hit, "Find", paragraph_index);
        }
    }
    return [cuts, cut_hit];
}

function fixPatches(cut, paragraph_index) {
    if (fixStageOn) {
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
    
    return [suggestions, edit_hit];
}

function verifyPatches(cut, edit_hit, suggestions, paragraph_index) {
    if (verifyStageOn) {
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
    return [grammar_votes, meaning_votes, vote_hit]
}

/**
 * Creates HITs to find shortenable regions in the paragraph
 */
function requestPatches(paragraph) {
	var text = getParagraph(paragraph);

	var webpage = s3.putString(slurp("../templates/shortn/shortn-find.html").replace(/___PARAGRAPH___/g, text).replace(/___ESCAPED_PARAGRAPH___/g, escape(text)));

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
	socketStatus(FIND_STAGE, status, paragraph_index);
	
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
	socketStatus(FIX_STAGE, status, paragraph_index);
	
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
	socketStatus(FILTER_STAGE, status, paragraph_index);
	
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
	
	return [grammar_votes, meaning_votes];
}

function generatePatch(cut, cut_hit, edit_hit, vote_hit, grammar_votes, meaning_votes, suggestions, paragraph_index) {
	var patch = {
		start: cut.start,   // beginning of the identified patch
		end: cut.end,
        editStart: cut.start,  // beginning of the region that revisions touch -- to be changed later in this function
        editEnd: cut.end,
		options: [],
		paragraph: paragraph_index,
        canCut: false,
        cutVotes: 0,
        numEditors: 0,
        merged: false,
        originalText: cut.plaintextSentence()
	}
    
    if (edit_hit != null) {
		var edit_hit = mturk.getHIT(edit_hit, true);
    }
	if (vote_hit != null) {
		var vote_hit = mturk.getHIT(vote_hit, true);
    }

	if (edit_hit != null) {
		cuttable_votes = get_vote(edit_hit.assignments, (function(answer) { return answer.cuttable; }));
		var numSayingCuttable = cuttable_votes['Yes'] ? cuttable_votes['Yes'] : 0;
		
		patch.canCut = ((numSayingCuttable / edit_hit.assignments.length) > .5);
		patch.cutVotes = numSayingCuttable;
		patch.numEditors = edit_hit.assignments.length;
	}
    
    if (suggestions != null) {
        var dmp = new diff_match_patch();
		for (var i = 0; i < suggestions.length; i++) {
			// this will be one of the alternatives they generated
			var newText = suggestions[i];

			var this_grammar_votes = grammar_votes[newText] ? grammar_votes[newText] : 0;
			var this_meaning_votes = meaning_votes[newText] ? meaning_votes[newText] : 0;
			var passesGrammar = (this_grammar_votes / vote_hit.assignments.length) < .5;
			var passesMeaning = (this_meaning_votes / vote_hit.assignments.length) < .5;
            
            // Now we calculate the beginning of the edit and the end of the edit region
            var diff = dmp.diff_main(cut.plaintextSentence(), newText);
            dmp.diff_cleanupSemantic(diff);
            
            var original_index = 0;
            var edit_start = -1;
            var edit_end = -1;
            for (var j = 0; j < diff.length; j++) {
                // if it's an insert or delete, and this is the first one, mark it
                if (diff[j][0] != 0 && edit_start == -1) { 
                    edit_start = original_index;
                }
                
                // if we are removing something, mark the end of the deletion as a possible last point
                if (diff[j][0] == -1) {
                    edit_end = original_index + diff[j][1].length;
                }
                // if we are adding something, mark the beginning of the insertion as a possible last point
                if (diff[j][0] == 1) {
                    edit_end = original_index;
                }                
                
                // if it's keeping it the same, or removing things, (meaning we're in the original string), increment the counter
                if (diff[j][0] == 0 || diff[j][0] == -1) {
                    original_index += diff[j][1].length;
                }
            }
            
            // we need to know what offset the cut starts at, by summing together the lengths of the previous sentences
            var editOffset = cut.sentences.slice(0, cut.sentenceRange().startSentence).join(Patch.sentence_separator).length;
            
			if (passesGrammar && passesMeaning) {
				patch.options.push({
                    text: newText,
                    editedText: newText,    // will be updated in a moment
                    editStart: edit_start + editOffset,
                    editEnd: edit_end + editOffset,
                    numVoters: vote_hit.assignments.length,
                    meaningVotes: this_meaning_votes,
                    grammarVotes: this_grammar_votes,
                    diff: diff
                });
			}
		}
	}
    
    if (patch.options.length > 0) {
        patch.options.sort( function(a, b) { return a.editStart - b.editStart; } ); // ascending by location of first edit
        patch.editStart = patch.options[0].editStart;
        patch.options.sort( function(a, b) { return b.editEnd - a.editEnd; } ); // descending by location of last edit
        patch.editEnd = patch.options[0].editEnd;
        
        // We make sure that the original patch location is at least covered by the edit area
        patch.editStart = Math.min(patch.editStart, patch.start);
        patch.editEnd = Math.max(patch.editEnd, patch.end);
        
        // For each option we need to edit it back down to just the changed portion, removing the extraenous parts of the sentence
        // e.g., we need to prune to just [patch.editStart, patch.editEnd]
        var editOffset = cut.sentences.slice(0, cut.sentenceRange().startSentence).join(Patch.sentence_separator).length;
        for (var i=0; i<patch.options.length; i++) {
            // To remove the extraneous parts of the text, we turn the first and last elements of the diff
            // (the prefix and postfix) into deletions
            var diff_cut = prune(patch.options[i].diff, 1000000);    // copy it very deep
            
            // First we remove the unnecessary parts of the prefix from the text, keeping only what everybody has edited
            var startOffset = patch.editStart - editOffset;
            var prefixCut = diff_cut[0][1].substring(0, startOffset);
            var prefixKeep = diff_cut[0][1].substring(startOffset);
            var cutStartDiffElement = [-1, prefixCut];   // -1 == delete
            var keepStartDiffElement = [0, prefixKeep];  // 0 == keep
            diff_cut.splice(0, 1, cutStartDiffElement, keepStartDiffElement); // remove the original first element and replace it with our cut and keep
            
            // Now we do the same with the end
            var endLength = cut.sentences.slice(0, cut.sentenceRange().endSentence+1).join(Patch.sentence_separator).substring(patch.editEnd).length;
            //print('end length: ' + endLength);
            var postfixString = diff_cut[diff_cut.length-1][1];
            var postfixKeep = postfixString.substring(0, postfixString.length - endLength);
            //print('postfix keep: ' + postfixKeep);
            var postfixCut = diff_cut[diff_cut.length-1][1].substring(postfixString.length - endLength);
            //print('postfix cut: ' + postfixCut);
            keepEndDiffElement = [0, postfixKeep];  // 0 == keep
            cutEndDiffElement = [-1, postfixCut];   // -1 == delete
            diff_cut.splice(diff_cut.length-1, 1, keepEndDiffElement, cutEndDiffElement); // remove the original first element and replace it with our cut and keep            
            
            //print('tweaked diff_cut:');
            //print(json(diff_cut));
            
            var editedText = dmp.patch_apply(dmp.patch_make(diff_cut), cut.plaintextSentence())[0];
            patch.options[i].editedText = editedText;
        }
    }
    // return to original sort order
    patch.options.sort( function(a, b) { return a.start - b.start; } );    
    return patch;
}

/**
 *  Writes human-readable and machine-readable information about thit HITs to disk.
 *  Can be turned off in a production system; this is for experiments and debugging.
 */
function outputEdits(output, lag_output, payment_output, paragraph, cut, cut_hit, edit_hit, vote_hit, grammar_votes, meaning_votes, suggestions, paragraph_index, patch)
{	
    if (!fileOutputOn) {
        return;
    }
    
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

    
	if (edit_hit != null) {
		output.write("<p>Is it cuttable?  <b>" + patch.cutVotes + "</b> of " + edit_hit.assignments.length + " turkers say yes.</p>");
	}
	
	var dmp = new diff_match_patch();    
	if (suggestions != null) {
		for (var i = 0; i < suggestions.length; i++) {
			// this will be one of the alternatives they generated
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
		}
	}   
}

function initializeOutput() {
    if (fileOutputOn) {
        output = new java.io.FileWriter("active-hits/shortn-results." + soylentJob + ".html");
        lag_output = new java.io.FileWriter("active-hits/shortn-" + soylentJob + "-fix_errors_lag.csv");
        lag_output.write("Stage,Assignment,Wait Type,Time,Paragraph\n");
        payment_output = new java.io.FileWriter("active-hits/shortn-" + soylentJob + "-fix_errors_payment.csv");
        payment_output.write("Stage,Assignment,Cost,Paragraph\n");
        patchesOutput = new java.io.FileWriter("active-hits/shortn-patches." + soylentJob +".json");
    }
}

function initializeDebug() {
	if (debug)
	{
		print('debug version');
		search_redundancy = 1;
		edit_redundancy = 1;
		verify_redundancy = 1;
        buffer_redundancy = 0;
		paragraphs = [ paragraphs[0] ]; 	//remove the parallelism for now
		wait_time = 5 * 1000;
		search_minimum_agreement = .6;
	}
}

function socketShortn(patch)
{
	sendSocketMessage("shortn", patch);
}

function finishedPatches(finishedArray) {
    // returns true if all array elements are true, e.g., all patches have been cut
    return finishedArray.reduce( function(previousValue, currentValue, index, array) {
        return previousValue && currentValue;
    });
}

/**
 * Looks for patches with overlapping edit bounds and merges them together for the purposes
 * of the user interface.
 */
function mergePatches(patches, paragraph_index) {
    print('merging...')
    patches.sort( function(a, b) { return a.editStart - b.editStart; } );
    var mergedPatches = new Array();
    
    var openPatch = 0;
    var openIndex = patches[0].editStart;
    var closeIndex = patches[0].editEnd;
    for (var i=0; i<patches.length; i++) {
        if (closeIndex < patches[i].editStart) {    // if we start a new region
            var mergedPatch = doMerge(patches, openPatch, i-1, paragraph_index);
            mergedPatches.push(mergedPatch);
            openPatch = i;
            openIndex = patches[i].editStart;
            closeIndex = patches[i].editEnd;
        } else {    // we need to mark this one as mergeable; it starts before the closeindex
            closeIndex = Math.max(closeIndex, patches[i].editEnd);
        }
    }
    // merge final open patch
    var mergedPatch = doMerge(patches, openPatch, patches.length-1, paragraph_index);
    mergedPatches.push(mergedPatch);    
    
    return mergedPatches;
}

function doMerge(patches, startPatch, endPatch, paragraph_index) {
    if (startPatch == endPatch) {
        print('Merging a singleton.');
        return patches[startPatch];
    }
    else {
        print('Merging ' + startPatch + ' to ' + endPatch);
        var newPatch = prune(startPatch, 10^10);    // do a deep copy of the object, 10^10 takes us to pretty much artibrary depth
        // get the largest end value of the patches
        newPatch.end = Array.max(map(patches.slice(startPatch, endPatch+1), function(patch) { return patch.end; } ) );
        newPatch.editStart = Array.min(map(patches.slice(startPatch, endPatch+1), function(patch) { return patch.editStart; } ) );
        newPatch.editEnd = Array.max(map(patches.slice(startPatch, endPatch+1), function(patch) { return patch.editEnd; } ) );
        newPatch.canCut = false;    // we're going to embed the individual cuttability estimates in the options, since now you can only cut part of the patch
        newPatch.cutVotes = 0;
        newPatch.numEditors = Stats.sum(map(patches.slice(startPatch, endPatch+1), function(patch) { return patch.numEditors; } ) );
        newPatch.merged = true;
        newPatch.options = new Array();
        
        for (var i=startPatch; i<=endPatch; i++) {
            newPatch.options = newPatch.options.concat(mergeOptions(patches, startPatch, endPatch, i, paragraph_index, newPatch.editStart, newPatch.editEnd));
        }
        return newPatch;
    }
}

function mergeOptions(patches, startPatch, endPatch, curPatch, paragraph_index, editStart, editEnd) {
    var options = new Array();
    
    print('\n\nPatch merging ' + curPatch);
    var prefix = getParagraph(paragraphs[paragraph_index]).substring(editStart, patches[curPatch].editStart);
    var postfix = getParagraph(paragraphs[paragraph_index]).substring(patches[curPatch].editEnd, editEnd);
    var dmp = new diff_match_patch();
    for (var i=0; i<patches[curPatch].options.length; i++) {
        var option = patches[curPatch].options[i];
        
        // diff[0] and diff[length-1] will always be the edges that are untouched, so we need to subtract them out
        var editRegion = option.text.slice(option.diff[0][1].length, -1 * option.diff[option.diff.length-1][1].length)
        
        var newOption = {
            text: prefix + editRegion + postfix,
            editStart: editStart,
            editEnd: editEnd,
            numVoters: option.numVoters,
            meaningVotes: option.meaningVotes,
            grammarVotes: option.grammarVotes
        }
        options.push(newOption);
    }
    
    if (patches[curPatch].canCut) {
        // create an option that cuts the entire original patch, if it was voted cuttable
        var prefix = getParagraph(paragraphs[paragraph_index]).substring(editStart, patches[curPatch].start);
        var postfix = getParagraph(paragraphs[paragraph_index]).substring(patches[curPatch].end, editEnd);

        var newOption = {
            text: prefix + postfix,
            editStart: editStart,
            editEnd: editEnd,
            numVoters: patches[curPatch].numEditors,
            meaningVotes: 0,    // not strictly correct, but hey, what can we do? TODO: we should merge cuttability to create an option earlier in the code before spinning off a verify step
            grammarVotes: 0
        }
        options.push(newOption);
    }
    
    return options;
}