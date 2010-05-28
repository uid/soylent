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

function get_HIT_URL(hit) {
	var question=new XML(hit.question);
	var url = question.*[0];
	return url;
}

/*
 * does not work yet due to memoization -- needs to call getHIT rather than waitforhit
 */
function boundedTimeWaitForHIT(hit, num_secs) {
	/*
	if (!hit.done && (new Date() - hit.creationTime) > maximum_wait) {
		var finalHIT = hit;
		mturk.deleteHIT(finalHIT);
	} else {	
		var finalHIT = mturk.waitForHIT(hit);
	}
	return finalHIT;
	*/
	return mturk.waitForHIT(hit);
}

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

function get_vote(assignments, extractVoteFromAnswer, multipleVotes) {
	if (!multipleVotes)
	{
		multipleVotes = false;
	}
	
	var votes = {}
	for(var i=0; i<assignments.length; i++)
	{
		var assignment = assignments[i];
		//print(json(assignment));
		var vote = extractVoteFromAnswer(assignment.answer)
		if (multipleVotes) {
			for (var j=0; j<vote.length; j++) {
				var thisVote = vote[j];
				votes[thisVote] = ensure(votes, [thisVote], 0) + 1
			}
		}
		else {
			votes[vote] = ensure(votes, [vote], 0) + 1
		}
	}
	return votes;
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

function getPaymentString(hit, title) {
	var hit_data = mturk.getHIT(hit, true);
	return (<div>{title} cost ${hit_data.reward} per each of {hit_data.assignments.length} workers = ${hit_data.reward * hit_data.assignments.length}</div>).toXMLString();
}

function getTimingString(hit, title) {
	var hit_data = mturk.getHIT(hit, true);
	
	results = []
	for (var i=0; i<hit_data.assignments.length; i++) {
		var result = hit_data.assignments[i];
		var elapsedTimeSeconds = (result.submitTime - result.acceptTime)/1000;
		var waitTimeSeconds = (result.acceptTime - hit_data.creationTime)/1000;

		// generate HTML highlighting the differences between the texts
		var result = <div>{title} task completed in {elapsedTimeSeconds} sec after a {waitTimeSeconds} wait for acceptance</div>;
		results.push(result.toXMLString());
	}
	return '<div>' + results.join('\n') + "</div>";
}

function writeCSVPayment(csv, hit, title)
{
	var hit_data = mturk.getHIT(hit, true);
	for (var i=0; i<hit_data.assignments.length; i++) {
		var out = []
		out.push(title);
		out.push(i+1);
		out.push(hit_data.reward);
		csv.write(out.join(",") + '\n');
	}
}

function writeCSVWait(csv, hit, title)
{
	var hit_data = mturk.getHIT(hit, true);
	for (var i=0; i<hit_data.assignments.length; i++) {
		var result = hit_data.assignments[i];
		var elapsedTimeSeconds = (result.submitTime - result.acceptTime)/1000;
		var waitTimeSeconds = (result.acceptTime - hit_data.creationTime)/1000;
		
		var work_out = [title, i+1, "work", elapsedTimeSeconds];
		csv.write(work_out.join(",") + '\n');
		var wait_out = [title, i+1, "wait", waitTimeSeconds];
		csv.write(wait_out.join(",") + '\n');
	}	
}

var FIND_STAGE = "find";
var FIX_STAGE = "fix";
var FILTER_STAGE = "filter";
function socketStatus(stage, numCompleted, paragraphNum) {
	if (typeof(soylentJob) == "undefined") {
		print("Not in socket mode, not writing.");
		return;
	}
	
	var message = {
		stage: stage,
		job: soylentJob,
		numCompleted: numCompleted,
		paragraph: paragraphNum
	};
	sendSocketMessage(message);
}

function sendSocketMessage(message) {
	if (socketOut == null) {
		print("Socket is not open, not writing.");
		return;
	}
	var stringMessage = json(message);
	stringMessage = stringMessage.substring(1, stringMessage.length-1);
	socketOut.println(stringMessage);
}