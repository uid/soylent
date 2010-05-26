// imports
eval(read("../library/patch.js"));
eval(read("../library/hit_utils.js"));
eval(read("../library/diff_match_patch_uncompressed.js"));

var search_reward = 0.06;
var search_redundancy = 10;

var edit_reward = 0.08;
var edit_redundancy = 5;  // number of turkers requested for each HIT

var verify_reward = 0.04;
var verify_redundancy = 5;

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
 *
var sentences = ["When I look at this picture of myself, I realize how much I've grown and changed, not only physically, but also mentally as a person in the last couple of years.", "Less than one month after this photograph was taken, I arrived at Woodbridge High School in Irvine, California without any idea of what to expect.", "I entered my second year of high school as an innocent thirteen year-old who was about a thousand miles from home and was a new member of not the sophomore, but \"lower-middle\" class.", "Around me in this picture are the things which were most important in my life at the time: studying different types of cars and planes, following every move made by Tiger Woods, and seeing the latest blockbuster movies like \"The Dark Knight\" or \"Spider Man 3.\"", "On my t-shirt is the rest of my life -- golf. Midway through my senior year at Woodbridge High School, the focuses in my life have changed dramatically."]
*/

/* 
var sentences = ["Who pays to build data sets?", "Good data sets are expensive to obtain.", "Particle physicists spend billions of dollars constructing particle accelerators just so they can record a few milliseconds of good data.", "But governments willingly provide the money and resources to help them gather this data because there isn’t a market for gluon data.", "There is, however, a market for your social networking behavior and web advertising clicks, so we shouldn’t hold our breaths waiting for the NSFs of the world to fund a multi-billion dollar social network just to gather behavioral data."];
*/

/*
 ORIGINAL
var paragraphs =	[
	[	"Print publishers are in a tizzy over Apple's new iPad because they hope to finally be able to charge for their digital editions.",
		"But in order to get people to pay for their magazine and newspaper apps, they are going to have to offer something different that readers cannot get at the newsstand or on the open Web.",
		"We've already seen plenty of prototypes  from magazine publishers which include interactive graphics, photo slide shows, and embedded videos."
	],
	
	[	"But what should a magazine cover look like on the iPad?",
		"After all, the cover is still the gateway to the magazine.",
		"Theoretically, it will still be the first page people see, giving them hints of what's inside and enticing them to dive into the issue.",
		"One way these covers could change is that instead of simply repurposing the static photographs from the print edition, the background image itself could be some sort of video loop.",
		"Jesse Rosten, a photographer in California, created the video mockup below of what a cover of Sunset Magazine might look like on the iPad (see video below)."
	],
	
	[	"The video shows ocean waves gently lapping a beach as the title of the magazine and other typographical elements appear on the page almost like movie credits.", 
		"He points out that these kinds of videos will have to be shot in a vertical orientation rather than a horizontal landscape one.",
		"This is just a mockup Rosten came up with on his own, but the designers of these new magazine apps should take note.",
		"The only way people are going to pay for these apps is if they create new experiences for readers."
	]
];
*/
/* round 2
var paragraphs =
[
	["Print publishers are in a tizzy over Apple's new iPad because they hope to charge for their digital editions.", "But to get people to pay for their magazine and newspaper apps, they must offer something that readers cannot get at the newsstand or free on the internet.","We've already seen plenty of prototypes from magazine publishers which include interactive graphics, photo slide shows, and embedded videos."],

	["But what should a magazine cover look like on the iPad?", "After all, the cover is still the gateway to the magazine.", "One way these covers could change is by using a video loop for the background image instead of the print edition's static photos.", "Jesse Rosten, a photographer in California, created the video mockup below of what a cover of Sunset Magazine might look like on the iPad (see video below)."],
	
	["The video shows ocean waves lapping a beach as typographical elements appear on the page almost like movie credits.", "He points out that these videos will have to be shot in a vertical orientation.", "This is just a mockup Rosten came up with on his own, but the designers of these new magazine apps should take note.", "The only way people are going to pay for these apps is if they create new experiences for readers."]
];
*/

var paragraphs =
[
	["Print publishers are in a tizzy over Apple's new iPad because they hope to charge for their digital editions.", "We've already seen plenty of prototypes from magazine publishers which include interactive graphics, photo slide shows, and embedded videos."],

	["But what should a magazine cover look like on the iPad?", "After all, the cover is still the gateway to the magazine.", "One way these covers could change is by using a video loop for the background image.", "Jesse Rosten, a photographer in California, created the video mockup below of what a cover of Sunset Magazine might look like on the iPad (see video below)."],
	
	["The video shows ocean waves lapping a beach as typographical elements appear on the page almost like movie credits.", "This is just a mockup Rosten came up with on his own, but the designers of these new magazine apps should take note.", "The only way people are going to pay for these apps is if they create new experiences for readers."]
];


main();





// main program
function main() {
	var output = new java.io.FileWriter("cut_results.html");
	for (var paragraph_index = 0; paragraph_index < paragraphs.length; paragraph_index++) {
		attempt(function() {
			print("paragraph #" + paragraph_index);
			print("requesting patches");
			var paragraph = paragraphs[paragraph_index];
			var cut_hit = requestPatches(paragraph);
			print("joining patches");
			cuts = joinPatches(cut_hit, paragraph);
			cleanUp(cut_hit);
			
			output.write(getPaymentString(cut_hit, "Patch Identification"));	
			output.write(getTimingString(cut_hit, "Patch Identification"));			
			
			for (var i=0; i<cuts.length; i++) {
				print('Patch #' + i + '\n\n');
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
					outputEdits(output, paragraph, cut, cut_hit, edit_hit, vote_hit, grammar_votes, meaning_votes);
					cleanUp(edit_hit);
					cleanUp(vote_hit);
				});
				print('\n\n\n');
			}
		});	
	}
	output.close();
}

/**
 * Creates HITs to find shortenable regions in the paragraph
 */
function requestPatches(paragraph) {
	var text = getParagraph(paragraph);

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
function joinPatches(cut_hit, paragraph) {
	var status = mturk.getHIT(cut_hit, true)
	print("completed by " + status.assignments.length + " turkers");
	var hit = mturk.waitForHIT(cut_hit)
	print("done! completed by " + hit.assignments.length + " turkers");
	
	var patch_suggestions = generatePatchSuggestions(hit.assignments, paragraph);
	//print(json(patch_suggestions));
	var patches = aggregatePatchSuggestions(patch_suggestions, paragraph);
	//print(json(patches));
	
	/*
	for (var i=0; i<patches.length; i++) {
		print(patches[i].highlightedSentence());
	}
	*/
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
			//print(suggestion.sentenceRange());
			//print(user_paragraph);
			//print(suggestion.highlightedSentence());
			//print('\n\n');
			suggestions.push(suggestion);
			numMatches++;
		}
	}
	suggestions.sort(function(a, b) { return (a.start - b.start); });
	return suggestions;
}

function aggregatePatchSuggestions(patch_suggestions, sentences) {
	var open = []
	var start = null, end = null;
	var patches = []
	
	for (var i=0; i<getParagraph(sentences).length; i++) {
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
	
	var dmp = new diff_match_patch();
	
	// provide a challenge if there is only one option
	if (options.length == 1) {
		var original = cut.plaintextSentence();
		if (original != options[0]) {
			options.push(original);
		}
		else {
			options.push("Warning: test option. Mechanical Turker, you will not be paid if you choose this option. Thanks!");
		}
	}	

	var t_grammar = '<table>';
	var t_meaning = '<table>';	
	foreach(options, function (correction, j) {
		var diff = dmp.diff_main(cut.plaintextSentence(), correction);
		dmp.diff_cleanupSemantic(diff);		
		var diff_html = "<div>" + dmp.diff_prettyHtml(diff) + "</div>";		
		
		var grammar_row = '<tr valign="top" class="grammar"><td><input type="radio" name="grammar" value="' + escape(correction) + '"></input></td><td>' +  diff_html + '</td></tr>';
		t_grammar += grammar_row;
		
		var meaning_row = '<tr valign="top" class="meaning"><td><input type="radio" name="meaning" value="' + escape(correction) + '"></input></td><td>' +  diff_html + '</td></tr>';
		t_meaning += meaning_row;
	});
	t_grammar += '</table>';
	t_meaning += '</table>';
	
	/*
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
	*/
	
	// Now we create a hit to vote on whether it's good
	var header = read("../library/hit_header.js").replace(/___BLOCK_WORKERS___/g, edit_workers)
					.replace(/___PAGE_NAME___/g, "shorten_vote");
					
	var webpage = s3.putString(slurp("vote_errors.html")
		.replace(/___HIGHLIGHTED___/g, cut.highlightedSentence())	
		.replace(/___GRAMMAR_VOTE___/g, t_grammar)
		.replace(/___MEANING_VOTE___/g, t_meaning)		
		.replace(/___HEADER_SCRIPT___/g, header));					
	
	/*
	var webpage = s3.putString(slurp("cut-verify.html").replace(/___HIGHLIGHTED___/g, cut.highlightedSentence())
					.replace(/___GRAMMAR_VOTE___/g, t_grammar).replace(/___MEANING_VOTE___/g, t_meaning)
					.replace(/___BLOCK_WORKERS___/g, edit_workers));
	*/
	
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
	return vote_hit;
}

function joinVotes(vote_hit) {
	// get the votes
	var hitId = vote_hit;
	var status = mturk.getHIT(hitId, true)	
	print("completed by " + status.assignments.length + " turkers");
	var hit = mturk.waitForHIT(hitId);
	print("done! completed by " + hit.assignments.length + " turkers");
	
	var grammar_votes = get_vote(hit.assignments, function(answer) { return unescape(answer.grammar); });
	var meaning_votes = get_vote(hit.assignments, function(answer) { return unescape(answer.meaning); });
	//print('grammar');
	//print(json(grammar_votes));
	//print('meaning');
	//print(json(meaning_votes));
	
	return [grammar_votes, meaning_votes];
}

function outputEdits(output, paragraph, cut, cut_hit, edit_hit, vote_hit, grammar_votes, meaning_votes)
{	
	output.write(preWrap(getParagraph(paragraph)));

	var edit_hit = mturk.getHIT(edit_hit, true)	
	var cut_hit = mturk.getHIT(cut_hit, true);
	var vote_hit = mturk.getHIT(vote_hit, true);
	
	output.write(getPaymentString(edit_hit, "Shortened Version Editing"));	
	output.write(getTimingString(edit_hit, "Shortened Version Editing"));	
	output.write(getPaymentString(vote_hit, "Voting"));	
	output.write(getTimingString(vote_hit, "Voting"));		
	
	/*
	output.write((<div>Patch identification cost ${cut_hit.reward} per each of {cut_hit.assignments.length} workers = ${cut_hit.reward * cut_hit.assignments.length}</div>).toXMLString());
	output.write((<div>Edits cost ${edit_hit.reward} per each of {edit_hit.assignments.length} workers = ${edit_hit.reward * edit_hit.assignments.length}</div>).toXMLString());	
	output.write((<div>Voting cost ${vote_hit.reward} per each of {vote_hit.assignments.length} workers = ${vote_hit.reward * vote_hit.assignments.length}</div>).toXMLString());
	*/
	output.write("<h1>Patch</h1>");
	output.write("<h2>Original</h2>" + preWrap(cut.highlightedSentence()));
	cuttable_votes = get_vote(edit_hit.assignments, (function(answer) { return answer.cuttable; }));
	var numSayingCuttable = cuttable_votes['Yes'] ? cuttable_votes['Yes'] : 0;
	output.write("<p>Is it cuttable?  <b>" + numSayingCuttable + "</b> of " + edit_hit.assignments.length + " turkers say yes.</p>");
	
	/*
	output.write("<ul>");
	for (var i = 0; i< cut_hit.assignments.length; i++) {
		var result = cut_hit.assignments[i]
		var elapsedTimeSeconds = (result.submitTime - result.acceptTime)/1000;
		var waitTimeSeconds = (result.acceptTime - cut_hit.creationTime)/1000;
		output.write("<li>Cut recommended in " + elapsedTimeSeconds + " sec after a " + waitTimeSeconds + " sec wait for acceptance</li>");
	output.write("</ul>");
	}*/
	
	var dmp = new diff_match_patch();
	
	for (var i = 0; i < edit_hit.assignments.length; i++) {
		// this will be one of the alternatives they generated
		var result = edit_hit.assignments[i]
		var newText = result.answer.newText
		/*
		var elapsedTimeSeconds = (result.submitTime - result.acceptTime)/1000;
		var waitTimeSeconds = (result.acceptTime - edit_hit.creationTime)/1000;
		*/
		
		var this_grammar_votes = grammar_votes[newText] ? grammar_votes[newText] : 0;
		var this_meaning_votes = meaning_votes[newText] ? meaning_votes[newText] : 0;

		// generate HTML highlighting the differences between the texts
		/*output.write(<div><h3>Version {i+1}</h3>
						  <p>produced in {elapsedTimeSeconds} sec
									  after a {waitTimeSeconds} wait for acceptance</p></div>);
									  
		output.write(<p><b>Edits made by this turker:</b></p>);
		output.write(preWrap(highlightDiffInColor(cut.plaintextSentence(), newText, "#80FF80").b));
		*/
		var diff = dmp.diff_main(cut.plaintextSentence(), newText);
		dmp.diff_cleanupSemantic(diff);		
		var diff_html = "<div>" + dmp.diff_prettyHtml(diff) + "</div>";		
		
		output.write(diff_html);
		output.write("<div>How many people thought this had the most grammar problems? <b>" + this_grammar_votes + "</b> of " + verify_redundancy + " turkers.</div>");
		output.write("<div>How many people thought this changed the meaning most? <b>" + this_meaning_votes + "</b> of " + verify_redundancy + " turkers.</div>");		
		output.flush();
	}
	
	/*
	for (var i=0; i < vote_hit.assignments.length; i++) {
		var result = vote_hit.assignments[i]
		var elapsedTimeSeconds = (result.submitTime - result.acceptTime)/1000;
		var waitTimeSeconds = (result.acceptTime - vote_hit.creationTime)/1000;		
		output.write("<li>Vote performed in " + elapsedTimeSeconds + " sec after a " + waitTimeSeconds + " sec wait for acceptance</li>");
	}
	*/
}

/*
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
*/