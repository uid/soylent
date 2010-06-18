eval(read("../library/find-fix-verify.js"));

var findFixVerifyOptions = {
    jobType: "shortn",
    paragraphs: paragraphs,
    buffer_redundancy: 2,	// number of extra assignments to create so that they don't get squatted.
    wait_time: 20 * 60 * 1000,
    time_bounded: true,
    find: {
        HIT_title: "Find unnecessary text",
        HIT_description: "I need to shorten my paragraph, and need opinions on what to cut.",
        HTML_template: "../templates/shortn/shortn-find.html",
        reward: 0.08,
        minimum_agreement: 0.20,
        redundancy: 10,
        minimum_workers: 6, 
    },
    fix: {
        HIT_title: "Shorten Rambling Text",
        HIT_description: "A sentence in my paper is too long and I need your help cutting out the fat.",
        HTML_template: "../templates/shortn/shortn-fix.html",
        reward: 0.05,
        redundancy: 5,
        minimum_workers: 3, 
    },
    verify: {
        HIT_title: "Did I shorten text correctly?",
        HIT_description: "I need to shorten some text -- which version is best?",
        HTML_template: "../templates/shortn/shortn-verify.html",
        reward: 0.04,
        minimum_agreement: 0.20,
        redundancy: 5,
        minimum_workers: 3, 
    },
    socket: new Socket("shortn", "localhost", 11000, 2000),
    output: null//outputEdits
};

/*
var wait_time = 20 * 60 * 1000;

var search_reward = 0.08;
var search_redundancy = 10;
var search_minimum_agreement = 0.20
var search_minimum_workers = 6;

var edit_reward = 0.05;
var edit_redundancy = 5;  // number of turkers requested for each HIT
var edit_minimum_workers = 3;

var verify_reward = 0.04;
var verify_redundancy = 5;
var verify_minimum_workers = 3;

var time_bounded = true;
var rejectedWorkers = []

var output;
var lag_output;
var payment_output;
var patchesOutput;

var overallFastestParagraph = Number.MAX_VALUE;
var overallSlowestParagraph = Number.MIN_VALUE;	

var fileOutputOn = false;

var client = null;

var socket = null; // TODO: create Socket.js class to manage all this state
var socketOut = null; // TODO: create Socket.js class to manage all this state

        var host = "localhost";
        var port = 11000;
        var timeout = 2000; // seconds
*/

function main() {
    initializeOutput(findFixVerifyOptions);	
    initializeDebug();
    
    if (typeof(soylentJob) == "undefined") {
        if (typeof(paragraphs) == "undefined") {
            paragraphs = [ ["This is the first sentence of the first paragraph."] ]; 
        }
    }
    if (typeof(debug) == "undefined") {
        var debug = false;
    }
    
    // do the main program, and if it has to wait, close the socket
    attempt(function() {
        findFixVerify(findFixVerifyOptions);
    });  
    findFixVerifyOptions.socket.close();
    closeOutputs(findFixVerifyOptions);
}

function initializeOutput(options) {
    if (options.output != null) {
        output = new java.io.FileWriter("active-hits/shortn-results." + soylentJob + ".html");
        lag_output = new java.io.FileWriter("active-hits/shortn-" + soylentJob + "-fix_errors_lag.csv");
        lag_output.write("Stage,Assignment,Wait Type,Time,Paragraph\n");
        payment_output = new java.io.FileWriter("active-hits/shortn-" + soylentJob + "-fix_errors_payment.csv");
        payment_output.write("Stage,Assignment,Cost,Paragraph\n");
        patchesOutput = new java.io.FileWriter("active-hits/shortn-patches." + soylentJob +".json");
    }
}

/**
 * Closes all the FileWriters.
 */
function closeOutputs(options) {
    if (options.output != null) {
        payment_output.close();
        lag_output.close();	
        output.close();
        patchesOutput.close();
    }
}

function initializeDebug() {
	if (debug)
	{
		print('debug version');
		findFixVerifyOptions.find.redundancy = 2;
		findFixVerifyOptions.find.minimum_workers = 1;
		findFixVerifyOptions.find.minimum_agreement = .0001;        
		findFixVerifyOptions.fix.redundancy = 2;
		findFixVerifyOptions.fix.minimum_workers = 1;
		findFixVerifyOptions.verify.redundancy = 2;
		findFixVerifyOptions.verify.minimum_workers = 1;
        findFixVerifyOptions.buffer_redundancy = 0;
		findFixVerifyOptions.paragraphs = [ paragraphs[0] ]; 	//remove the parallelism for now
		findFixVerifyOptions.wait_time = 0 * 1000;
	}
}

/**
 *  Writes human-readable and machine-readable information about thit HITs to disk.
 *  Can be turned off in a production system; this is for experiments and debugging.
 */
function outputEdits(output, lag_output, payment_output, paragraph, patch, find_hit, edit_hit, verify_hit, grammar_votes, meaning_votes, suggestions, paragraph_index, outputPatch)
{	
	output.write(preWrap(getParagraph(paragraph)));

	if (find_hit != null) {
		var find_hit = mturk.getHIT(find_hit, true);
        output.write(getPaymentString(find_hit, "Find"));	
        output.write(getTimingString(find_hit, "Find"));

        writeCSVPayment(payment_output, find_hit, "Find", paragraph_index);
        writeCSVWait(lag_output, find_hit, "Find", paragraph_index);        
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
	
	if (verify_hit != null) {
		var verify_hit = mturk.getHIT(verify_hit, true);
		output.write(getPaymentString(verify_hit, "Voting"));	
		output.write(getTimingString(verify_hit, "Voting"));				
		output.write(getPaymentString(verify_hit, "Vote"));
		output.write(getTimingString(edit_hit, "Vote"));

		writeCSVPayment(payment_output, verify_hit, "Voting on Alternatives", paragraph_index);
		writeCSVWait(lag_output, verify_hit, "Voting on Alternatives", paragraph_index);		
	}
	else {
		print("OUTPUTTING NO FILTER HIT");
	}
	
	output.write("<h1>Patch</h1>");
	output.write("<h2>Original</h2>" + preWrap(patch.highlightedSentence()));

    
	if (edit_hit != null) {
		output.write("<p>Is it cuttable?  <b>" + outputPatch.cutVotes + "</b> of " + edit_hit.assignments.length + " turkers say yes.</p>");
	}
	
	var dmp = new diff_match_patch();    
	if (suggestions != null) {
		for (var i = 0; i < suggestions.length; i++) {
			// this will be one of the alternatives they generated
			var newText = suggestions[i];
			
			var this_grammar_votes = grammar_votes[newText] ? grammar_votes[newText] : 0;
			var this_meaning_votes = meaning_votes[newText] ? meaning_votes[newText] : 0;

			var diff = dmp.diff_main(patch.plaintextSentence(), newText);
			dmp.diff_cleanupSemantic(diff);		
			var diff_html = "<div>" + dmp.diff_prettyHtml(diff) + "</div>";		
			
			output.write(diff_html);
			output.write("<div>How many people thought this had the most grammar problems? <b>" + this_grammar_votes + "</b> of " + verify_hit.assignments.length + " turkers.</div>");
			output.write("<div>How many people thought this changed the meaning most? <b>" + this_meaning_votes + "</b> of " + verify_hit.assignments.length + " turkers.</div>");		
			output.flush();
		}
	}   
    
    patchesOutput.write(json(outputPatch));
}