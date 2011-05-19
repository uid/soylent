eval(read("../library/hit_utils.js"));
eval(read("../library/socket.js"));

var write_output = false;
var socket = new Socket("human-macro", "localhost", 11000, 2000);

function main()
{
	socket.connect();
    if (debug) {
        redundancy = 1;
    }

    if (write_output) {
        var output = new java.io.FileWriter("macro_results.html");
    }
	for (var j = 0; j < inputs.length; j++) {
		attempt(function() {
			print("work unit #" + j);
			var input = inputs[j];
			var work_hit = requestWork(input);
			print("joining work");
			var results = joinWork(work_hit, input, j);
			cleanUp(work_hit);
			print(json(results));
			
            if (write_output) {
                output.write(getPaymentString(work_hit, "Human Macro"));	
                output.write(getTimingString(work_hit, "Human Macro"));			
            }
            
            var macroResult = {
				input: j,
				alternatives: results
			};
            socket.sendMessage("complete", macroResult);
			
			print('\n\n\n');
		});	
	}
    
    if (write_output) {
        output.close();
    }
    socket.close();
}

function requestWork(input) {
	// Now we create a hit to vote on whether it's good
	var header = read("../library/hit_header.js")
					.replace(/___BLOCK_WORKERS___/g, [])
					.replace(/___PAGE_NAME___/g, "human_macro");

	var webpage = s3.putString(slurp("../templates/human-macro/macro.html")
					.replace(/___HEADER_SCRIPT___/g, header)
					.replace(/___INPUT___/g, input)
					.replace(/___INSTRUCTIONS___/g, instructions));
	
	// create a HIT on MTurk using the webpage
	var hitId = createHIT({
		title : title,
		desc : subtitle,
		url : webpage,
		height : 1200,
		assignments: redundancy,
		reward : reward,
		autoApprovalDelayInSeconds : 60 * 10,	// 10 minutes
		assignmentDurationInSeconds: 60 * 10,
		socket: socket
	})
	return hitId;
}

function joinWork(hit_id, input, index) {
	var status = mturk.getHIT(hit_id, true)
	print("completed by " + status.assignments.length + " turkers");
    socket.sendStatus("human-macro", status, index, 0, 1, 0);
    
	var hit = mturk.waitForHIT(hit_id)
	print("done! completed by " + hit.assignments.length + " turkers");
	
	var results = [];
	
	foreach(hit.assignments, function (assignment, i) {		
		if (assignment.answer.userText == input) {
			print("REJECTING: Just copied and pasted the prompt!");
            try {
                mturk.rejectAssignment(assignment, "Just copied and pasted the prompt!");
            } catch(e) {
                print(e);
            }            
		} else {
			results.push(assignment.answer.userText);
		}
	});
	
	return results;
}