// imports
eval(read("../library/patch.js"));
eval(read("../library/hit_utils.js"));
eval(read("../library/diff_match_patch_uncompressed.js"));
eval(read("../library/socket.js"));

/*
 *  Input data structure should look like:
 *  {
 *      paragraphs: [[string]] -- array of sentence strings (input),
 *      buffer_redundancy: int -- 2, or the number of extra assignments to create so that they don't get squatted,
 *      wait_time: int -- 20 * 60 * 1000, or the number of milliseconds to wait for each stage before timing out and continuing  with fewer workers than desired,
 *      time_bounded: boolean -- true if we want to put a timeout on how long we'll wait for any particular stage
 *      find: {
 *          HIT_title: string -- the title of the HIT that Turkers will see
 *          HIT_description: string -- the short description of the HIT that Turkers will see
 *          HTMLTemplate: string -- filename of HTML file to show Turkers for the Find stage
 *          reward: double -- 0.01, or another value of how much to pay each turker,
 *          minimum_agreement: double -- 0.20, or another percentage of how many turkers must agree on a patch to use it,
 *          redundancy: int -- 10, or the number of workers we want to complete the task
 *          minimum_workers: int -- the smallest number of workers to use. Wait for at least this many even if we time out.
 *      },
 *      fix: {
 *          HIT_title: string,
 *          HIT_description: string,
 *          HTML_template: string,
 *          reward: double,
 *          redundancy: int,
 *          minimum_workers: int, 
 *      },
 *      verify: {
 *          HIT_title: string,
 *          HIT_description: string, 
 *          HTML_template: string,
 *          reward: double,
 *          minimum_agreement: double,
 *          redundancy: int,
 *          minimum_workers: int, 
 *      },
 *      socket: a Socket object (see socket.js) if you would like status messages sent over the socket, or null if you don't want to write to the socket
 *      output: function pointer to function that is responsible for writing outputs to disk, or null if you don't want to write output to disk,
 */
 
/**
 * Performs a Find-Fix-Verify computation on the input text.
 */
function findFixVerify(options) {	
	var result = {
		paragraphs: []
	};
    var rejectedWorkers = [];
	
	for (var paragraph_index = 0; paragraph_index < options.paragraphs.length; paragraph_index++) {
		attempt(function() {
			print('\n\n\n');		
			print("paragraph #" + paragraph_index);
			var paragraph = options.paragraphs[paragraph_index];
            
            // Find stage
            [patches, find_hit] = findPatches(paragraph_index, options, rejectedWorkers);
			
            // Keep track of which paragraphs have finished their computation
            var finishedArray = new Array();
            for (var i=0; i<patches.length; i++) {
                finishedArray[i] = false;
            }
            var paragraphResult = {
				paragraph: paragraph_index,
				patches: []
			};
            
            var fixHITs = new Array();
            var verifyHITs = new Array();
			for (var i=0; i<patches.length; i++) {
				print('Patch #' + i + '\n\n');
				var patch = patches[i];
                
				attempt(function() {
                    // Fix stage
                    [suggestions, fix_hit] = fixPatches(patch, paragraph_index, i, patches.length, options, rejectedWorkers);
                    fixHITs[i] = fix_hit;
                    // Verify stage
                    [grammar_votes, meaning_votes, verify_hit] = verifyPatches(patch, fix_hit, suggestions, paragraph_index, i, patches.length, options, rejectedWorkers);
                    verifyHITs[i] = verify_hit;
					
                    // Create output data structure
                    var patchOutput = generatePatch(patch, find_hit, fix_hit, verify_hit, grammar_votes, meaning_votes, suggestions, paragraph_index);
                    paragraphResult.patches.push(patchOutput);
                    
                    // Write file output
                    if (options.output != null) {
                        options.output(output, lag_output, payment_output, paragraph, patch, find_hit,
                                    fix_hit, verify_hit, grammar_votes, meaning_votes, suggestions, paragraph_index, patchOutput);
                    }
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
            paragraphResult.patches = findOverlapsAndMerge(paragraphResult.patches, paragraph_index);
			paragraphResult.patches.sort( function(a, b) { return a.start - b.start; } );
			result.paragraphs.push(paragraphResult);
            
            options.socket.sendMessage("complete", paragraphResult);
            outputTimingData(patches, find_hit, fixHITs, verifyHITs);
		});	
	}
	
	print(json(result));
	
	if (rejectedWorkers.length > 0) {
		print("Rejected workers:");
		print(json(rejectedWorkers.sort()));
	}
}

//
// High-level stage methods: find, fix, verify
//

/**
 * Find stage. Finds patches according to the user's need (e.g., shortenable, misspelled, etc.)
 */
function findPatches(paragraph_index, findFixVerifyOptions, rejectedWorkers) {
    var find_hit = requestPatches(paragraph_index, findFixVerifyOptions);
    var patches = [];

    /* wait to get at least one patch out of the system --- if there's not enough agreement, ask for more people */
    while (true) {	
        patches = joinPatches(find_hit, paragraph_index, findFixVerifyOptions, rejectedWorkers);	
        if (patches.length > 0) {
            break;
        }
        else {
            extendHit(find_hit, buffer_redundancy);
        }	
    }
    cleanUp(find_hit);
    findFixVerifyOptions.socket.sendStageComplete(Socket.FIND_STAGE, paragraph_index, mturk.getHIT(find_hit, true), 0, 1);

    return [patches, find_hit];
}

/**
 * Fix stage. Asks Turkers to fix any patches that were found in the previous stage.
 */
function fixPatches(patch, paragraph_index, patchNumber, totalPatches, findFixVerifyOptions, rejectedWorkers) {
    var fix_hit = requestFixes(patch, findFixVerifyOptions);        
    var suggestions = []
    while (true) {	
        suggestions = joinFixes(fix_hit, patch.plaintextSentence(), paragraph_index, patchNumber, totalPatches, rejectedWorkers);
        if (suggestions.length > 0) {
            break;
        }
        else {
            extendHit(fix_hit, buffer_redundancy, patchNumber);
        }	
    }		
    
    cleanUp(fix_hit);
    findFixVerifyOptions.socket.sendStageComplete(Socket.FIX_STAGE, paragraph_index, mturk.getHIT(fix_hit, true), patchNumber, totalPatches);
    
    return [suggestions, fix_hit];
}

/**
 * Verify stage. Vets changes that were made and filters out bad ones.
 */
function verifyPatches(patch, fix_hit, suggestions, paragraph_index, patchNumber, totalPatches, findFixVerifyOptions, rejectedWorkers) {
    var verify_hit = requestVotes(patch, suggestions, fix_hit, findFixVerifyOptions);
    var grammar_votes = [];
    var meaning_votes = [];
    while (true) {
        [grammar_votes, meaning_votes] = joinVotes(verify_hit, paragraph_index, patchNumber, totalPatches, findFixVerifyOptions, rejectedWorkers);
        
        if (numVotes(grammar_votes) > 0 && numVotes(meaning_votes) > 0) {
            break;
        }
        else {
            extendHit(verify_hit, buffer_redundancy);
        }	
    }				
    
    cleanUp(verify_hit);
    findFixVerifyOptions.socket.sendStageComplete(Socket.VERIFY_STAGE, paragraph_index, mturk.getHIT(verify_hit, true), patchNumber, totalPatches);
    return [grammar_votes, meaning_votes, verify_hit]
}

//
// Find stage helper methods
//

/**
 * Creates HITs to find patches in the paragraph
 */
function requestPatches(paragraph_index, findFixVerifyOptions) {
	var text = getParagraph(findFixVerifyOptions.paragraphs[paragraph_index]);

	var webpage = s3.putString(slurp(findFixVerifyOptions.find.HTML_template).replace(/___PARAGRAPH___/g, text).replace(/___ESCAPED_PARAGRAPH___/g, escape(text)));

	// create a HIT on MTurk using the webpage
	var hitId = mturk.createHIT({
		title : findFixVerifyOptions.find.HIT_title,
		desc : findFixVerifyOptions.find.HIT_description,
		url : webpage,
		height : 1200,
		assignments: findFixVerifyOptions.find.redundancy + 2 * findFixVerifyOptions.buffer_redundancy,
		reward : findFixVerifyOptions.find.reward,
		autoApprovalDelayInSeconds : 60 * 60,
		assignmentDurationInSeconds: 60 * 5		
	})
	return hitId;
}

/**
 * Waits for the patch HITs, then merges them.
 */
function joinPatches(find_hit, paragraph_index, findFixVerifyOptions, rejectedWorkers) {
	var status = mturk.getHIT(find_hit, true)
	print("completed by " + status.assignments.length + " of " + findFixVerifyOptions.find.minimum_workers + " turkers");
	findFixVerifyOptions.socket.sendStatus(Socket.FIND_STAGE, status, paragraph_index, 0, 1);
	
	var hit = mturk.boundedWaitForHIT(find_hit, findFixVerifyOptions.wait_time, findFixVerifyOptions.find.minimum_workers, findFixVerifyOptions.find.redundancy);

	var patch_suggestions = generatePatchSuggestions(hit.assignments, findFixVerifyOptions.paragraphs[paragraph_index], rejectedWorkers);
	var patches = aggregatePatchSuggestions(patch_suggestions, hit.assignments.length, findFixVerifyOptions.paragraphs[paragraph_index]);

	print('\n\n\n');

	return patches;
}

var MAX_PATCH_LENGTH = 250;
/**
 * Identifies the areas in [[brackets]] and does error checking. If it's up to snuff, creates a Patch object for each [[area]].
 */
function generatePatchSuggestions(assignments, paragraph, rejectedWorkers) {
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

/**
 * Passes through all suggested patches and merges any overlapping ones into a single Patch.
 * Tosses out any patches that don't meet the minimum agreement requirements.
 */
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

//
// Fix helper methods
//

/**
 * Takes in patches of cuttable areas and spawns HITs to gather alternatives.
 */
function requestFixes(patch, findFixVerifyOptions) {
	var full_text = patch.highlightedParagraph();
	var editable = patch.plaintextSentence();

	var webpage = s3.putString(findFixVerifyOptions.fix.HTML_template.replace(/___TEXT___/g, full_text)
					.replace(/___EDITABLE___/g, editable));
	

	// create a HIT on MTurk using the webpage
	var fix_hit = mturk.createHIT({
		title : findFixVerifyOptions.fix.HIT_title,
		desc : findFixVerifyOptions.fix.HIT_description,
		url : webpage,
		height : 800,
		assignments: findFixVerifyOptions.fix.redundancy + findFixVerifyOptions.buffer_redundancy,
		reward : findFixVerifyOptions.fix.reward,
		autoApprovalDelayInSeconds : 60 * 60,
		assignmentDurationInSeconds: 60 * 5
	})
	return fix_hit;
}

/**
 * Waits for all the edits to be completed
 * @return: all the unique strings that turkers suggested
 */
function joinFixes(fix_hit, originalSentence, paragraph_index, patchNumber, totalPatches, rejectedWorkers) {
	var hitId = fix_hit;
	print("checking to see if HIT is done")
	var status = mturk.getHIT(hitId, true)	
	print("completed by " + status.assignments.length + " of " + edit_minimum_workers + " turkers");
	findFixVerifyOptions.socket.sendStatus(Socket.FIX_STAGE, status, paragraph_index, patchNumber, totalPatches);
	
	var hit = mturk.boundedWaitForHIT(hitId, wait_time, edit_minimum_workers, edit_redundancy);
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

//
// Vote helper methods
//

/**
 * Requests a vote filter for the options based on user-requested requirements (e.g., grammaticality)
 */
function requestVotes(patch, options, fix_hit, findFixVerifyOptions) {		
	// Disallow workers from the edit hits from working on the voting hits
	edit_workers = []
	for each (var asst in fix_hit.assignments) { 
		if (asst.workerId) edit_workers.push(asst.workerId); 
	}
	
	var dmp = new diff_match_patch();
	
	// provide a challenge if there is only one option
	if (options.length == 1) {
		var original = patch.plaintextSentence();
		if (original != options[0]) {
			options.push(original);
		}
	}	

    // Annotate the patch to make clear what's changed via a diff
	var t_grammar = '<table>';
	var t_meaning = '<table>';	
	foreach(options, function (correction, j) {
		var diff = dmp.diff_main(patch.plaintextSentence(), correction);
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
					
	var webpage = s3.putString(slurp(findFixVerifyOptions.verify.HTML_template)
		.replace(/___HIGHLIGHTED___/g, patch.highlightedParagraph())	
		.replace(/___GRAMMAR_VOTE___/g, t_grammar)
		.replace(/___MEANING_VOTE___/g, t_meaning)		
		.replace(/___HEADER_SCRIPT___/g, header));					
	
	// create a HIT on MTurk using the webpage
	var verify_hit = mturk.createHIT({
		title : findFixVerifyOptions.verify.HIT_title,
		desc : findFixVerifyOptions.verify.HIT_description,
		url : webpage,
		height : 800,
		assignments: findFixVerifyOptions.verify.redundancy + findFixVerifyOptions.buffer_redundancy, 
		reward : findFixVerifyOptions.verify.reward,
		autoApprovalDelayInSeconds : 60 * 60,
		assignmentDurationInSeconds: 60 * 5
	})
	return verify_hit;
}

/**
 * Error checks the vote stage and returns the vote score for each option.
 */
function joinVotes(verify_hit, paragraph_index, patchNumber, totalPatches, findFixVerifyOptions, rejectedWorkers) {
	// get the votes
	var hitId = verify_hit;
	var status = mturk.getHIT(hitId, true)	
	print("completed by " + status.assignments.length + " of " + verify_minimum_workers + " turkers");
	findFixVerifyOptions.socket.sendStatus(Socket.VERIFY_STAGE, status, paragraph_index, patchNumber, totalPatches);
	
	var hit = mturk.boundedWaitForHIT(hitId, findFixVerifyOptions.wait_time, findFixVerifyOptions.verify.minimum_workers, findFixVerifyOptions.verify.redundancy);
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

/**
 * Puts together a complete data structure that contains all the options, voting results, and more. 
 * Call this on a patch that has made it through the Verify stage.
 */
function generatePatch(patch, find_hit, edit_hit, verify_hit, grammar_votes, meaning_votes, suggestions, paragraph_index) {
	var outputPatch = {
		start: patch.start,   // beginning of the identified patch
		end: patch.end,
        editStart: patch.start,  // beginning of the region that revisions touch -- to be changed later in this function
        editEnd: patch.end,
		options: [],
		paragraph: paragraph_index,
        canCut: false,
        cutVotes: 0,
        numEditors: 0,
        merged: false,
        originalText: patch.plaintextSentence()   // also to be changed once we know editStart and editEnd
	}
    
    if (edit_hit != null) {
		var edit_hit = mturk.getHIT(edit_hit, true);
    }
	if (verify_hit != null) {
		var verify_hit = mturk.getHIT(verify_hit, true);
    }

	if (edit_hit != null) {
		cuttable_votes = get_vote(edit_hit.assignments, (function(answer) { return answer.cuttable; }));
		var numSayingCuttable = cuttable_votes['Yes'] ? cuttable_votes['Yes'] : 0;
		
		outputPatch.canCut = ((numSayingCuttable / edit_hit.assignments.length) > .5);
		outputPatch.cutVotes = numSayingCuttable;
		outputPatch.numEditors = edit_hit.assignments.length;
	}
    
    if (suggestions != null) {
        var dmp = new diff_match_patch();
		for (var i = 0; i < suggestions.length; i++) {
			// this will be one of the alternatives they generated
			var newText = suggestions[i];

			var this_grammar_votes = grammar_votes[newText] ? grammar_votes[newText] : 0;
			var this_meaning_votes = meaning_votes[newText] ? meaning_votes[newText] : 0;
			var passesGrammar = (this_grammar_votes / verify_hit.assignments.length) < .5;
			var passesMeaning = (this_meaning_votes / verify_hit.assignments.length) < .5;
            
            // Now we calculate the beginning of the edit and the end of the edit region
            var diff = dmp.diff_main(patch.plaintextSentence(), newText);
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
            
            // we need to know what offset the patch starts at, by summing together the lengths of the previous sentences
            var editOffset = patch.sentences.slice(0, patch.sentenceRange().startSentence).join(sentence_separator).length;
            if (patch.sentenceRange().startSentence > 0) {
                editOffset += sentence_separator.length;   // add the extra space after the previous sentences and before this one.
            }
            
			if (passesGrammar && passesMeaning) {
				outputPatch.options.push({
                    text: newText,
                    editedText: newText,    // will be updated in a moment
                    editStart: edit_start + editOffset,
                    editEnd: edit_end + editOffset,
                    numVoters: verify_hit.assignments.length,
                    meaningVotes: this_meaning_votes,
                    grammarVotes: this_grammar_votes,
                    diff: diff
                });
			}
		}
	}
    
    var previousSentences = patch.sentences.slice(0, patch.sentenceRange().startSentence);
    previousSentences.push(""); // to simulate the sentence that we're starting
    var editOffset = previousSentences.join(sentence_separator).length;
    if (outputPatch.options.length > 0) {
        outputPatch.options.sort( function(a, b) { return a.editStart - b.editStart; } ); // ascending by location of first edit
        outputPatch.editStart = outputPatch.options[0].editStart;
        outputPatch.options.sort( function(a, b) { return b.editEnd - a.editEnd; } ); // descending by location of last edit
        outputPatch.editEnd = outputPatch.options[0].editEnd;
        
        // We make sure that the original patch location is at least covered by the edit area
        outputPatch.editStart = Math.min(outputPatch.editStart, outputPatch.start);
        outputPatch.editEnd = Math.max(outputPatch.editEnd, outputPatch.end);
        
        // For each option we need to edit it back down to just the changed portion, removing the extraenous parts of the sentence
        // e.g., we need to prune to just [patch.editStart, patch.editEnd]        
        for (var i=0; i<outputPatch.options.length; i++) {
            // To remove the extraneous parts of the text, we turn the first and last elements of the diff
            // (the prefix and postfix) into deletions
            var diff_cut = prune(outputPatch.options[i].diff, 1000000);    // copy it very deep
            
            // First we remove the unnecessary parts of the prefix from the text, keeping only what everybody has edited
            if (diff_cut[0][0] == 0) {
                var startOffset = outputPatch.editStart - editOffset;
                var prefixCut = diff_cut[0][1].substring(0, startOffset);
                var prefixKeep = diff_cut[0][1].substring(startOffset);
                var cutStartDiffElement = [-1, prefixCut];   // -1 == delete
                var keepStartDiffElement = [0, prefixKeep];  // 0 == keep
                diff_cut.splice(0, 1, cutStartDiffElement, keepStartDiffElement); // remove the original first element and replace it with our cut and keep
            }
            
            // Now we do the same with the end
            if (diff_cut[diff_cut.length-1][0] == 0) {
                var endLength = patch.sentences.slice(0, patch.sentenceRange().endSentence+1).join(sentence_separator).substring(outputPatch.editEnd).length;
                var postfixString = diff_cut[diff_cut.length-1][1];
                var postfixKeep = postfixString.substring(0, postfixString.length - endLength);
                var postfixCut = diff_cut[diff_cut.length-1][1].substring(postfixString.length - endLength);
                keepEndDiffElement = [0, postfixKeep];  // 0 == keep
                cutEndDiffElement = [-1, postfixCut];   // -1 == delete
                diff_cut.splice(diff_cut.length-1, 1, keepEndDiffElement, cutEndDiffElement); // remove the original first element and replace it with our cut and keep            
            }
            
            var editedText = dmp.patch_apply(dmp.patch_make(diff_cut), patch.plaintextSentence())[0];
            outputPatch.options[i].editedText = editedText;
        }
    }
    
    outputPatch.originalText = outputPatch.originalText.substring(outputPatch.editStart - editOffset, outputPatch.editEnd - editOffset);
    
    // return to original sort order
    outputPatch.options.sort( function(a, b) { return a.start - b.start; } );    
    return outputPatch;
}

//
// Meta-stage helper methods
//

/**
 * Looks for patches with overlapping edit bounds and merges them together for the purposes
 * of the user interface.
 */
function findOverlapsAndMerge(patches, paragraph_index) {
    print('merging...')
    patches.sort( function(a, b) { return a.editStart - b.editStart; } );
    var mergedPatches = new Array();
    
    var openPatch = 0;
    var openIndex = patches[0].editStart;
    var closeIndex = patches[0].editEnd;
    for (var i=0; i<patches.length; i++) {
        if (closeIndex < patches[i].editStart) {    // if we start a new region
            var mergedPatch = mergePatches(patches, openPatch, i-1, paragraph_index);
            mergedPatches.push(mergedPatch);
            openPatch = i;
            openIndex = patches[i].editStart;
            closeIndex = patches[i].editEnd;
        } else {    // we need to mark this one as mergeable; it starts before the closeindex
            closeIndex = Math.max(closeIndex, patches[i].editEnd);
        }
    }
    // merge final open patch
    var mergedPatch = mergePatches(patches, openPatch, patches.length-1, paragraph_index);
    mergedPatches.push(mergedPatch);    
    
    return mergedPatches;
}

/**
 * 
 */
function mergePatches(patches, startPatch, endPatch, paragraph_index) {
    if (startPatch == endPatch) {
        return patches[startPatch];
    }
    else {
        print('Merging ' + startPatch + ' to ' + endPatch);
        var newPatch = prune(startPatch, 10^10);    // do a deep copy of the object, 10^10 takes us to pretty much artibrary depth
		newPatch.start = patches[startPatch].start;
        newPatch.end = Array.max(map(patches.slice(startPatch, endPatch+1), function(patch) { return patch.end; } ) );         // get the largest end value of the patches
        newPatch.editStart = Array.min(map(patches.slice(startPatch, endPatch+1), function(patch) { return patch.editStart; } ) );
        newPatch.editEnd = Array.max(map(patches.slice(startPatch, endPatch+1), function(patch) { return patch.editEnd; } ) );
        newPatch.canCut = false;    // we're going to embed the individual cuttability estimates in the options, since now you can only cut part of the patch
        newPatch.cutVotes = 0;
        newPatch.numEditors = Stats.sum(map(patches.slice(startPatch, endPatch+1), function(patch) { return patch.numEditors; } ) );
        newPatch.merged = true;
        newPatch.options = new Array();
		newPatch.originalText = getParagraph(paragraphs[paragraph_index]).substring(newPatch.editStart, newPatch.editEnd);
        
        for (var i=startPatch; i<=endPatch; i++) {
            newPatch.options = newPatch.options.concat(mergeOptions(patches, startPatch, endPatch, i, paragraph_index, newPatch.editStart, newPatch.editEnd));
        }
        return newPatch;
    }
}

/**
 * Merges the replacement options for each patch
 */
function mergeOptions(patches, startPatch, endPatch, curPatch, paragraph_index, editStart, editEnd) {
    var options = new Array();
    
    print('\n\nPatch merging ' + curPatch);
	print(json(patches[curPatch]))
    var prefix = getParagraph(paragraphs[paragraph_index]).substring(editStart, patches[curPatch].editStart);
    var postfix = getParagraph(paragraphs[paragraph_index]).substring(patches[curPatch].editEnd, editEnd);
	
    var dmp = new diff_match_patch();
    for (var i=0; i<patches[curPatch].options.length; i++) {
        var option = patches[curPatch].options[i];
        
        // diff[0] and diff[length-1] will always be the edges that are untouched, so we need to subtract them out
        var editRegion = option.text.slice(option.diff[0][1].length, -1 * option.diff[option.diff.length-1][1].length)
        
        var newOption = {
            text: prefix + editRegion + postfix,
            editedText: prefix + editRegion + postfix,   // already cropped to the correct region
            editStart: editStart,
            editEnd: editEnd,
            numVoters: option.numVoters,
            meaningVotes: option.meaningVotes,
            grammarVotes: option.grammarVotes,
			originalText: getParagraph(paragraphs[paragraph_index]).substring(editStart, editEnd)
        }
        options.push(newOption);
    }
    
    if (patches[curPatch].canCut) {
        // create an option that cuts the entire original patch, if it was voted cuttable
        var prefix = getParagraph(paragraphs[paragraph_index]).substring(editStart, patches[curPatch].start);
        var postfix = getParagraph(paragraphs[paragraph_index]).substring(patches[curPatch].end, editEnd);

        var newOption = {
            text: prefix + postfix,
			editedText: prefix + postfix,
            editStart: editStart,
            editEnd: editEnd,
            numVoters: patches[curPatch].numEditors,
            meaningVotes: 0,    // not strictly correct, but hey, what can we do? TODO: we should merge cuttability to create an option earlier in the code before spinning off a verify step
            grammarVotes: 0,
			originalText: getParagraph(paragraphs[paragraph_index]).substring(editStart, editEnd)
        }
        options.push(newOption);
    }
    
    return options;
}

/**
 * Returns true if all array elements are true, e.g., all patches have been cut
 */
function finishedPatches(finishedArray) {
    return finishedArray.reduce( function(previousValue, currentValue, index, array) {
        return previousValue && currentValue;
    });
}

/**
 *  Prints out information to the console concerning how quickly all the tasks were completed.
 */
function outputTimingData(patches, find_hit, fixHITs, verifyHITs) {
    var findTime = getHITEndTime(find_hit) - getHITStartTime(find_hit);
    var maxFixVerifyTime = Number.MIN_VALUE;
    var minFixVerifyTime = Number.MAX_VALUE;
    var overallFastestParagraph = Number.MAX_VALUE;
    var overallSlowestParagraph = Number.MIN_VALUE;	    
    for (var i=0; i<patches.length; i++) {
        var fix_hit = fixHITs[i];
        var verify_hit = verifyHITs[i];
        var fixTime = getHITEndTime(fix_hit) - getHITStartTime(fix_hit);
        var verifyTime = getHITEndTime(verify_hit) - getHITStartTime(verify_hit);
        maxFixVerifyTime = Math.max(fixTime+verifyTime, maxFixVerifyTime);
        minFixVerifyTime = Math.min(fixTime+verifyTime, minFixVerifyTime);
        overallFastestParagraph = Math.min(overallFastestParagraph, (findTime + maxFixVerifyTime));
        overallSlowestParagraph = Math.max(overallSlowestParagraph, (findTime + maxFixVerifyTime));		        
    }
    
    print("Find time: " + findTime);
    print('Longest Fix+Verify: ' + maxFixVerifyTime);
    print('Shortest Fix+Verify: ' + minFixVerifyTime);
    print('Max Elapsed time (seconds): ' + (findTime + maxFixVerifyTime) / 1000);
    print('Max Elapsed time (minutes): ' + ((findTime + maxFixVerifyTime) / (1000*60)));
    print('Min Elapsed time (seconds): ' + (findTime + minFixVerifyTime) / 1000);
    print('Min Elapsed time (minutes): ' + ((findTime + minFixVerifyTime) / (1000*60)));			
}