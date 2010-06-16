eval(read("../library/find-fix-verify.js"));

var buffer_redundancy = 2;	// number of extra assignments to create so that they don't get squatted.
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

function main() {
    setupSocket();
    initializeOutput();	
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
    attempt(shortn);  
    teardownSocket();
}

function initializeOutput() {
    output = new java.io.FileWriter("active-hits/shortn-results." + soylentJob + ".html");
    lag_output = new java.io.FileWriter("active-hits/shortn-" + soylentJob + "-fix_errors_lag.csv");
    lag_output.write("Stage,Assignment,Wait Type,Time,Paragraph\n");
    payment_output = new java.io.FileWriter("active-hits/shortn-" + soylentJob + "-fix_errors_payment.csv");
    payment_output.write("Stage,Assignment,Cost,Paragraph\n");
    patchesOutput = new java.io.FileWriter("active-hits/shortn-patches." + soylentJob +".json");
}

function initializeDebug() {
	if (debug)
	{
		print('debug version');
		search_redundancy = 2;
		search_minimum_workers = 1;
		edit_redundancy = 2;
		edit_minimum_workers = 1;
		verify_redundancy = 2;
		verify_minimum_workers = 1;
        buffer_redundancy = 0;
		paragraphs = [ paragraphs[0] ]; 	//remove the parallelism for now
		wait_time = 20 * 1000;
		search_minimum_agreement = .0001;
	}
}

function socketShortn(patch)
{
	sendSocketMessage("shortn", patch);
}