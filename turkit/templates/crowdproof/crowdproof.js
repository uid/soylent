eval(read("../library/find-fix-verify.js"));

var findFixVerifyOptions = {
    jobType: "crowdproof",
    paragraphs: paragraphs,
    buffer_redundancy: 2,	// number of extra assignments to create so that they don't get squatted.
    wait_time: 20 * 60 * 1000,
    time_bounded: true,
    find: {
		HIT_title : "Find bad writing",
		HIT_description : "This paragraph needs some help finding errors. You're far better than Microsoft Word's grammar checker.",
        HTML_template: "../templates/crowdproof/crowdproof-find.html",
        reward: 0.06,
        minimum_agreement: 0.20,
        redundancy: 10,
        minimum_workers: 6,
        transformWebpage: crowdproofFindTransformWebpage,
        customTest: null
    },
    fix: {
        HIT_title: "Shorten Rambling Text",
        HIT_description: "A sentence in my paper is too long and I need your help cutting out the fat.",
        HTML_template: "../templates/crowdproof/crowdproof-fix.html",
        reward: 0.08,
        redundancy: 5,
        minimum_workers: 3, 
        transformWebpage: null,
        customTest: crowdproofFixTest,
        mapResults: null,        
    },
    verify: {
		HIT_title : "Vote on writing suggestions",
		HIT_description : "I have several rewrites of this sentence. Which one is best?",
        HTML_template: "../templates/crowdproof/crowdproof-verify.html",
        reward: 0.04,
        minimum_agreement: 0.20,
        redundancy: 5,
        minimum_workers: 3,
        fields: [
            {
                name: 'fix',
                fixFormElement: 'revision',
                passes: function(numVotes, totalVotes) { return (numVotes / totalVotes) >= .3; },
            },
            {
                name: 'reason',
                fixFormElement: 'reason',
                passes: function(numVotes, totalVotes) { return (numVotes / totalVotes) >= .3; },
            }
        ],
        editedTextField: 'revision',
        customTest: null,
        transformWebpage: null,
        mapResults: crowdproofMapVerifyResults,
    },
    socket: new Socket("crowdproof", "localhost", 11000, 2000),
    writeOutput: false    
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

function crowdproofFindTransformWebpage(webpageContents, paragraph) {
    return webpageContents.replace(/___ESCAPED_PARAGRAPH___/g, escape(paragraph))
}

function crowdproofFixTest(toTest) {
    var correction = toTest.answer.revision;
    var reason = toTest.answer.reason;
    var originalSentence = toTest.patch.plaintextSentence();    
    
    if (correction == originalSentence) {
        return {
                    passes: false,
                    reason: "Please do not copy/paste the original sentence back in. We're looking for a corrected version."
                };
    }
    else if (correction == null || correction == "") {
        return {
                    passes: false,
                    reason: "Your correction is an empty form."
                };
    }
    else if (reason == null || reason == "") {
        return {
                    passes: false,
                    reason: "You did not provide a reason for the error."
                };
    }    
    else {
        return {
                    passes: true,
                    reason: ""
                };
    }
}

function crowdproofMapVerifyResults(fieldOption) {
    if (fieldOption.editsText) {
        var toRemove = []
        foreach(fieldOption.alternatives, function(alternative, index) {
            if (alternative.editStart == -1 && alternative.editEnd == -1) {
                // it was a copy of the original
                toRemove.push(alternative);
            }
        });
        foreach(toRemove, function(alternative) {
            fieldOption.alternatives.splice(fieldOption.alternatives.indexOf(alternative), 1);
        });
    }
}