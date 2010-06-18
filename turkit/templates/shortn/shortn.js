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
        transformWebpage: shortnFindTransformWebpage,
        customTest: null
    },
    fix: {
        HIT_title: "Shorten Rambling Text",
        HIT_description: "A sentence in my paper is too long and I need your help cutting out the fat.",
        HTML_template: "../templates/shortn/shortn-fix.html",
        reward: 0.05,
        redundancy: 5,
        minimum_workers: 3,
        transformWebpage: null,
        customTest: shortnFixTest,
        mapFixResults: shortnMapFixResults,
    },
    verify: {
        HIT_title: "Did I shorten text correctly?",
        HIT_description: "I need to shorten some text -- which version is best?",
        HTML_template: "../templates/shortn/shortn-verify.html",
        reward: 0.04,
        minimum_agreement: 0.20,
        redundancy: 5,
        minimum_workers: 3,
        fields: ['grammar', 'meaning'],
        customTest: null,
    },
    socket: new Socket("shortn", "localhost", 11000, 2000),
    writeOutput: true
};

function main() {
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

function shortnFindTransformWebpage(webpageContents, paragraph) {
    return webpageContents.replace(/___ESCAPED_PARAGRAPH___/g, escape(paragraph))
}

function shortnFixTest(answer) {
    var text = e.answer.newText;
    if (text == originalSentence) {
        return {
                    passes: false,
                    reason: "Please do not copy/paste the original sentence back in. We're looking for a shorter version."
                };
    }
    else if (text.length >= originalSentence) {
        return {
                    passes: false,
                    reason: "Your sentence was as long or longer than the original. We're looking for a shorter version."
                };
    }
    else {
        return {
                    passes: true,
                    reason: ""
                };
    }
}

function shortnMapFixResults(answers, patch) {
    var results = Array();
    
    var cutVotes = 0
    foreach(answers, function(answer) {
        results.push(answer.newText);
        if (answer.cuttable) {
            cutVotes++;
        }
    });
    
    if (cutVotes >= .5 * answers.length) {
        results.push(patch.getCutSentence());
    }
    
    return results.unique();
}