var search_reward = .01; //0.06;
var search_redundancy = 10;

var edit_reward = .01;//0.08;
var edit_redundancy = 4;  // number of turkers requested for each HIT

var verify_reward = .01//0.04;
var verify_redundancy = 5;

var sentence_separator = "  ";

/*
 * original paragraph
var sentences = ["Automatic clustering generally helps separate different kinds of records that need to be edited differently, but it isn't perfect.",
"Sometimes it creates more clusters than needed, because the differences in structure aren't important to the user's particular editing task.",
"For example, if the user only needs to edit near the end of each line, then differences at the start of the line are largely irrelevant, and it isn't necessary to split based on those differences.",
"Conversely, sometimes the clustering isn't fine enough, leaving heterogeneous clusters that must be edited one line at a time.",
"One solution to this problem would be to let the user rearrange the clustering manually, perhaps using drag-and-drop to merge and split clusters.",
"Clustering and selection generalization would also be improved by recognizing common text structure like URLs, filenames, email addresses, dates, times, etc."];
*/

/**
 * second round
var sentences = ["Automatic clustering generally helps separate different kinds of records that need to be edited differently, but it isn't perfect.",
"Sometimes it creates more clusters than needed, as structure differences aren't important to the user's particular editing task.",
"For example, if the user only needs to edit near the end of each line, then differences at the start of the line are largely irrelevant.",
"Conversely, sometimes the clustering isn't fine enough, leaving heterogeneous clusters that must be edited one line at a time.",
"One solution to this problem would be to let the user rearrange the clustering manually.",
"Clustering and selection generalization would also be improved by recognizing common text structure like URLs, filenames, email addresses, dates, times, etc."];
*/

/*
 * college essay
 */
var sentences = ["When I look at this picture of myself, I realize how much I've grown and changed, not only physically, but also mentally as a person in the last couple of years.", "Less than one month after this photograph was taken, I arrived at Woodbridge High School in Irvine, California without any idea of what to expect.", "I entered my second year of high school as an innocent thirteen year-old who was about a thousand miles from home and was a new member of not the sophomore, but \"lower-middle\" class.", "Around me in this picture are the things which were most important in my life at the time: studying different types of cars and planes, following every move made by Tiger Woods, and seeing the latest blockbuster movies like \"The Dark Knight\" or \"Spider Man 3.\"", "On my t-shirt is the rest of my life -- golf. Midway through my senior year at Woodbridge High School, the focuses in my life have changed dramatically."]


/* 
var sentences = ["Who pays to build data sets?", "Good data sets are expensive to obtain.", "Particle physicists spend billions of dollars constructing particle accelerators just so they can record a few milliseconds of good data.", "But governments willingly provide the money and resources to help them gather this data because there isn’t a market for gluon data.", "There is, however, a market for your social networking behavior and web advertising clicks, so we shouldn’t hold our breaths waiting for the NSFs of the world to fund a multi-billion dollar social network just to gather behavioral data."];
*/

function getParagraph() {
	return sentences.join(sentence_separator);
}

//var text = "Crowdsourcing platforms now allow us to integrate humans permanently into computation, extending the abilities of that technology and thus of the interface. Whereas Wizard of Oz techniques use humans as a temporary, stopgap solution for incomplete algorithms, we envision entire interactive systems relying on an ever-present worker crowd. Such humans can solve problems that algorithms cannot, opening up a new class of interfaces. An interactive system can now use humans to solve difficult problems or to work in collaboration with an algorithm to reduce error.";

// Classes
function Cuttable(start, end) {
	this.start = start;
	this.end = end;
}

/*
 * Start and end bounds on the sentences the Cuttable spans
 */
Cuttable.prototype.sentenceRange = function() {
	var startSentence = null;
	var endSentence = null;
	var currentText = null;
	for (var i=0; i<sentences.length; i++) {
		currentText = sentences.slice(0, i+1).join(sentence_separator);
		if (currentText.length >= this.start && startSentence == null) {
			startSentence = i;
			startPosition = this.start - (currentText.length - sentences[i].length);
		}
		if (currentText.length >= this.start + (this.end - this.start)) {
			endSentence = i;
			endPosition = this.end - (currentText.length - sentences[i].length);
			break;
		}
	}	
	return {
		startPosition: startPosition,
		startSentence: startSentence,
		endPosition: endPosition,
		endSentence: endSentence
	};
}

Cuttable.prototype.highlightedSentence = function() {
	var range = this.sentenceRange();
	var sentence = ""
	for (var i=range.startSentence; i<=range.endSentence; i++) {
		if (i == range.startSentence) {	
			sentence = sentences[i].substring(0, range.startPosition);
			sentence += "<span style='background-color: yellow;'>"
			if (range.startSentence != range.endSentence) {
				sentence += sentences[i].substring(range.startPosition);
			}
		}
		if (i == range.endSentence) {
			var end_sentence_start;
			if (range.startSentence == range.endSentence) {
				end_sentence_start = range.startPosition
			}
			else {
				end_sentence_start = 0;
			}
			sentence += sentences[i].substring(end_sentence_start, range.endPosition);	// add everything
			sentence += "</span>";
			sentence += sentences[i].substring(range.endPosition);
		}
		if (i != range.startSentence && i != range.endSentence) {
			sentence += sentence_separator + sentences[i];
		}
	}
	return sentence;
};

Cuttable.prototype.highlightedParagraph = function() {
	var range = this.sentenceRange();
	
	var paragraph_sentences = []
	paragraph_sentences = paragraph_sentences.concat(sentences.slice(0, range.startSentence));
	paragraph_sentences = paragraph_sentences.concat(this.highlightedSentence());
	if (range.endSentence < sentences.length) {
		paragraph_sentences = paragraph_sentences.concat(sentences.slice(range.endSentence+1));
	}
	return paragraph_sentences.join(sentence_separator);
};

Cuttable.prototype.plaintextSentence = function() {
	var range = this.sentenceRange();
	return sentences.slice(range.startSentence, range.endSentence+1).join(sentence_separator);
};

Array.prototype.unique = function () {
	var r = new Array();
	o:for(var i = 0, n = this.length; i < n; i++)
	{
		for(var x = 0, y = r.length; x < y; x++)
		{
			if(r[x]==this[i])
			{
				continue o;
			}
		}
		r[r.length] = this[i];
	}
	return r;
}

main();





// main program
function main() {
	var output = new java.io.FileWriter("cut_results.html");
	print("requesting patches");
	var cut_hit = requestPatches();
	print("joining patches");
	cuts = joinPatches(cut_hit);
	cleanUp(cut_hit);
	
	for (var i=0; i<cuts.length; i++) {
		print('Patch #' + i);
		var cut = cuts[i];
		attempt(function() {
			print("requesting edits");
			var edit_hit = requestEdits(cut);
			print("joining edits");
			var suggestions = joinEdits(edit_hit);
			print("requesting votes");
			var vote_hit = requestVotes(cut, suggestions, edit_hit);
			print("joining votes");
			[grammar_votes, meaning_votes] = joinVotes(vote_hit);
			outputEdits(output, cut, cut_hit, edit_hit, vote_hit, grammar_votes, meaning_votes);
			cleanUp(edit_hit);
			cleanUp(vote_hit);
		});
		print('\n\n\n');
	}
	output.close();
}

/**
 * Creates HITs to find shortenable regions in the paragraph
 */
function requestPatches() {
	var text = getParagraph();

	var webpage = s3.putString(slurp("identify-patches.html").replace(/___PARAGRAPH___/g, text).replace(/___ESCAPED_PARAGRAPH___/g, escape(text)));
	
	// create a HIT on MTurk using the webpage
	var hitId = mturk.createHIT({
		title : "Find unnecessary text",
		desc : "I need to shorten my paragraph, and need opinions on what to cut.",
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
function joinPatches(cut_hit) {
	var status = mturk.getHIT(cut_hit, true)
	print("completed by " + status.assignments.length + " turkers");
	var hit = mturk.waitForHIT(cut_hit)
	print("done! completed by " + hit.assignments.length + " turkers");
	
	var patch_suggestions = generatePatchSuggestions(hit.assignments);
	print(json(patch_suggestions));
	var patches = aggregatePatchSuggestions(patch_suggestions);
	print(json(patches));
	
	for (var i=0; i<patches.length; i++) {
		print(patches[i].highlightedSentence());
	}
	print('\n\n\n');
	/*
	var cuts = [
		new Cuttable(73, 107, 0, 130, text),
		new Cuttable(599, 627, 599, 744, text),
		new Cuttable(846, 902, 746, 903, text),
	];
	*/
	return patches;
}

function generatePatchSuggestions(assignments) {
	var suggestions = [];
	print("todo: what if they add text beyond four angle brackets? e.g., write something or add a space. maybe check for this in the HIT by seeing if edit distance is only insertions of [[ and ]] and a simple FSM makes sure that they match.  Only allowed to add [[ and ]] to the text, nothing else.");
	for (var i=0; i<assignments.length; i++) {
		var user_paragraph = assignments[i].answer.brackets;
		var brackets = /\[\[(.*?)\]\]/g;
		
		var numMatches = 0;
		while((match = brackets.exec(user_paragraph)) != null) {
			var start_index = match.index - (4 * numMatches);	// subtract out [['s
			var end_index = start_index + match[1].length;
			var suggestion = new Cuttable(start_index, end_index);
			print(suggestion.sentenceRange());
			print(user_paragraph);
			print(suggestion.highlightedSentence());
			print('\n\n');
			suggestions.push(suggestion);
			numMatches++;
		}
	}
	suggestions.sort(function(a, b) { return (a.start - b.start); });
	return suggestions;
}

function aggregatePatchSuggestions(patch_suggestions) {
	var open = []
	var start = null, end = null;
	var patches = []
	
	for (var i=0; i<getParagraph().length; i++) {
		for (var j=0; j<patch_suggestions.length; j++) {
			if (i == patch_suggestions[j].start) {
				open.push(patch_suggestions[j]);
				//print(open.length);
				if (open.length == 2 && start == null) {
					start = open[0].start;
				}
			}

			if (i == patch_suggestions[j].end) {
				open.splice(open.indexOf(open[j]), 1);
				//print(open.length);
				if (open.length == 0 && start != null) {
					end = i;
					patches.push(new Cuttable(start, end));
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
	var highlighted_sentence = 	cut.highlightedSentence();
	var full_text = cut.highlightedParagraph();
	var editable = cut.plaintextSentence();

	var webpage = s3.putString(slurp("cut.html").replace(/___HIGHLIGHTED___/g, highlighted_sentence).replace(/___TEXT___/g, full_text)
					.replace(/___EDITABLE___/g, editable));
	

	// create a HIT on MTurk using the webpage
	var edit_hit = mturk.createHIT({
		title : "Shorten Rambling Text",
		desc : "A sentence in my paper is too long and I need your help cutting out the fat.",
		url : webpage,
		height : 800,
		assignments: edit_redundancy,
		reward : edit_reward,
		autoApprovalDelayInSeconds : 60,
		assignmentDurationInSeconds: 60 * 5
	})
	print(edit_hit)
	return edit_hit;
}

/*
 * Waits for all the edits to be completed
 * @return: all the unique strings that turkers suggested
 */
function joinEdits(edit_hit) {
	var hitId = edit_hit;
	print("checking to see if HIT is done")
	var status = mturk.getHIT(hitId, true)	
	print("completed by " + status.assignments.length + " turkers");
	var hit = mturk.waitForHIT(hitId)
	print("done! completed by " + hit.assignments.length + " turkers");
	
	var options = new Array();
	foreach(hit.assignments, function(e) { options.push(e.answer.newText) });
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
	
	var t_grammar = <table/>
	var t_meaning = <table/>
	foreach(options, function (e, j) {
		var text_diff = new XML("<span>" + highlightDiff(cut.plaintextSentence(), e).b + "</span>");
		var row = <tr valign="top" class="random"><td><input type="radio" value={e}></input></td><td>{text_diff}</td></tr>;

		var grammar_row = row.copy()
		grammar_row.@["class"] = "random grammar";
		grammar_row..input.@name = "grammar"
		t_grammar.appendChild(grammar_row);

		var meaning_row = row.copy()
		meaning_row.@["class"] = "random meaning";
		meaning_row..input.@name = "meaning"
		t_meaning.appendChild(meaning_row);
	});	
	
	// Now we create a hit to vote on whether it's good
	var webpage = s3.putString(slurp("cut-verify.html").replace(/___HIGHLIGHTED___/g, cut.highlightedSentence())
					.replace(/___GRAMMAR_VOTE___/g, t_grammar).replace(/___MEANING_VOTE___/g, t_meaning)
					.replace(/___BLOCK_WORKERS___/g, edit_workers));
	
	// create a HIT on MTurk using the webpage
	var vote_hit = mturk.createHIT({
		title : "Did I shorten text correctly?",
		desc : "I need to shorten some text -- which version is best?",
		url : webpage,
		height : 800,
		assignments: verify_redundancy, 
		reward : verify_reward,
		autoApprovalDelayInSeconds : 60,
		assignmentDurationInSeconds: 60 * 5
	})
	print(vote_hit)
	return vote_hit;
}

function joinVotes(vote_hit) {
	// get the votes
	var hitId = vote_hit;
	print("checking to see if HIT is done")
	var status = mturk.getHIT(hitId, true)	
	print("completed by " + status.assignments.length + " turkers");
	var hit = mturk.waitForHIT(hitId);
	print("done! completed by " + hit.assignments.length + " turkers");
	
	var grammar_votes = verify_vote(hit.assignments, function(answer) { return answer.grammar; });
	var meaning_votes = verify_vote(hit.assignments, function(answer) { return answer.meaning; });
	print('grammar');
	print(json(grammar_votes));
	print('meaning');
	print(json(meaning_votes));
	
	return [grammar_votes, meaning_votes];
}

function outputEdits(output, cut, cut_hit, edit_hit, vote_hit, grammar_votes, meaning_votes)
{	
	output.write(preWrap(getParagraph()));

	var edit_hit = mturk.getHIT(edit_hit, true)	
	var cut_hit = mturk.getHIT(cut_hit, true);
	var vote_hit = mturk.getHIT(vote_hit, true);
	
	output.write((<div>Patch identification cost ${cut_hit.reward} per each of {cut_hit.assignments.length} workers = ${cut_hit.reward * cut_hit.assignments.length}</div>).toXMLString());
	output.write((<div>Edits cost ${edit_hit.reward} per each of {edit_hit.assignments.length} workers = ${edit_hit.reward * edit_hit.assignments.length}</div>).toXMLString());	
	output.write((<div>Voting cost ${vote_hit.reward} per each of {vote_hit.assignments.length} workers = ${vote_hit.reward * vote_hit.assignments.length}</div>).toXMLString());
	
	output.write("<h1>Patch</h1>");
	output.write("<h2>Original</h2>" + preWrap(cut.highlightedSentence()));
	cuttable_votes = verify_vote(edit_hit.assignments, (function(answer) { return answer.cuttable; }));
	var numSayingCuttable = cuttable_votes['Yes'] ? cuttable_votes['Yes'] : 0;
	output.write("<p>Is it cuttable?  <b>" + numSayingCuttable + "</b> of " + edit_hit.assignments.length + " turkers say yes.</p>");
	output.write("<ul>");
	for (var i = 0; i< cut_hit.assignments.length; i++) {
		var result = cut_hit.assignments[i]
		var elapsedTimeSeconds = (result.submitTime - result.acceptTime)/1000;
		var waitTimeSeconds = (result.acceptTime - cut_hit.creationTime)/1000;
		output.write("<li>Cut recommended in " + elapsedTimeSeconds + " sec after a " + waitTimeSeconds + " sec wait for acceptance</li>");
	}
	output.write("</ul>");
	
	for (var i = 0; i < edit_hit.assignments.length; i++) {
		// this will be one of the alternatives they generated
		var result = edit_hit.assignments[i]
		var newText = result.answer.newText
		var elapsedTimeSeconds = (result.submitTime - result.acceptTime)/1000;
		var waitTimeSeconds = (result.acceptTime - edit_hit.creationTime)/1000;
		
		var this_grammar_votes = grammar_votes[newText] ? grammar_votes[newText] : 0;
		var this_meaning_votes = meaning_votes[newText] ? meaning_votes[newText] : 0;

		// generate HTML highlighting the differences between the texts
		var diff = 
		output.write(<div><h3>Version {i+1}</h3>
						  <p>produced in {elapsedTimeSeconds} sec
									  after a {waitTimeSeconds} wait for acceptance</p></div>);
									  
		output.write(<p><b>Edits made by this turker:</b></p>);
		output.write(preWrap(highlightDiffInColor(cut.plaintextSentence(), newText, "#80FF80").b));
		output.write("<p>How many people thought this had the most grammar problems? <b>" + this_grammar_votes + "</b> of " + verify_redundancy + " turkers.</p>");
		output.write("<p>How many people thought this changed the meaning most? <b>" + this_meaning_votes + "</b> of " + verify_redundancy + " turkers.</p>");		
		output.flush();
	}
	
	for (var i=0; i < vote_hit.assignments.length; i++) {
		var result = vote_hit.assignments[i]
		var elapsedTimeSeconds = (result.submitTime - result.acceptTime)/1000;
		var waitTimeSeconds = (result.acceptTime - vote_hit.creationTime)/1000;		
		output.write("<li>Vote performed in " + elapsedTimeSeconds + " sec after a " + waitTimeSeconds + " sec wait for acceptance</li>");
	}
}

/**
 * Deletes all HIT resources
 */
function cleanUp(hitId) {
	var hit = mturk.getHIT(hitId, true)	
	var url = get_HIT_URL(hit);

	// first, let's delete the original HIT
	mturk.deleteHIT(hit)

	// we also created a page on S3,
	// so delete it
	s3.deleteObject(url)
}

function preWrap(html) {
  return <div><pre style="width:500px;border:thin solid; white-space: pre-wrap; white-space: -moz-pre-wrap; white-space: -o-pre-wrap;">__DIFF__</pre></div>
     .toString().replace(/__DIFF__/, html);
}

function highlightDiffInColor(a, b, color) {
  diff = highlightDiff(a, b);
  diff.a = diff.a.replace(/background-color:yellow/g, "background-color:" + color);
  diff.b = diff.b.replace(/background-color:yellow/g, "background-color:" + color);
  return diff;
}

function verify_vote(assignments, extractVoteFromAnswer) {
	var votes = {}
	foreach(assignments, function(assignment) {
				var vote = extractVoteFromAnswer(assignment.answer)
				votes[vote] = ensure(votes, [vote], 0) + 1
			})		
	//var winnerVotes, winner
	//[winnerVotes, winner] = getMax(votes)
	//return getMax(votes)
	return votes;
}

function get_HIT_URL(hit) {
	var question=new XML(hit.question);
	var url = question.*[0];
	return url;
}