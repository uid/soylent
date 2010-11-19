/**
 * A wrapper around mturk.createHIT. Catches errors like
 * running out of money, and returns them to the client.
 */
function createHIT(hitOptions)
{
	var exceptionSocket = hitOptions.socket;
	delete hitOptions.socket;
	
	try {
		return mturk.createHIT(hitOptions);
	} catch (e) {
		if (exceptionSocket != null) {
			print("Throwing exception to C#")
			
			var codeRegex = /<Code>(.*)<\/Code>/g
			var match = codeRegex.exec(e);
			
			if (match != null) {
				var code = match[1];
				exceptionSocket.sendException(code, e);
			} 
		}
			
		// halt execution
		java.lang.System.exit(1);
		//throw e;
	}
}

/**
 * Deletes all HIT resources
 */
function cleanUp(hitId) {
	var hit = mturk.getHIT(hitId, true);
	var url = get_HIT_URL(hit);

	// first, let's delete the original HIT
	mturk.deleteHIT(hit);

	// we also created a page on S3,
	// so delete it
	s3.deleteObject(url);
}

function get_HIT_URL(hit) {
	var question=new XML(hit.question);
	var url = question.*[0];
	return url;
}

/**
 * A fork of waitForHIT that puts an upper limit on the amount of time we're willing to wait.
 */
MTurk.prototype.boundedWaitForHIT = function(hit, maximumWait, minWorkers, maxWorkers) {
	if (typeof(time_bounded) != "undefined" && !time_bounded) {
		// if the programmer has turned off bounds
		print("we are not time-bounded. waiting as long as we need.");
		
		if (typeof(maxWorkers) == "undefined") {
			maxWorkers = hit.maxAssignments;
		}

		var hit = mturk.getHIT(hit, true);		
		if (hit.assignments.length < maxWorkers) {
			print('not enough workers yet. unbounded wait.');
			stop();
		} else {
			return hit;
		}
	}

	if (minWorkers == undefined) {
		minWorkers = 1;
	}
	
	var me = mturk
	var hitId = mturk.tryToGetHITId(hit)
	return once(function() {
		// the idea of this logic
		// is to minimize the number of calls to MTurk
		// to see if HITs are done.
		// 
		// if we are going to be calling waitForHIT a lot,
		// then we'd like to get a list of all reviewable HITs,
		// and check for the current HIT against that list,
		// and refresh that list only if enough time has passed.
		//
		// of course, if the list of reviewable HITs is very long,
		// then we'd rather not retrieve it,
		// unless we will be calling this function a lot,
		// so to figure out how many times we should wait before
		// retrieving the list,
		// we start by seeing how many pages of results that list has,
		// and if we call this function that many times,
		// then we go ahead and get the list
		
		if (!me.waitForHIT_callCount) {
			me.waitForHIT_callCount = 0
			var a = me.getReviewableHITs(1)
			if (a.totalNumResults == a.length) {
				me.waitForHIT_reviewableHITs = new Set(a)
				me.waitForHIT_reviewableHITsTime = time()
			}
			me.waitForHIT_waitCount = Math.ceil(a.totalNumResults / 100)
		}
		me.waitForHIT_callCount++
		if (me.waitForHIT_callCount >= me.waitForHIT_waitCount) {
			if (!me.waitForHIT_reviewableHITs ||
				(time() > me.waitForHIT_reviewableHITsTime + (1000 * 60))) {
				me.waitForHIT_reviewableHITs = new Set(me.getReviewableHITs())
				me.waitForHIT_reviewableHITsTime = time()
			}
		}
		if (me.waitForHIT_reviewableHITs) {
			if (!me.waitForHIT_reviewableHITs[hitId]) {
				return boundedStop(hitId, maximumWait, minWorkers, maxWorkers);
			}
		}
		

		var hit = mturk.getHIT(hitId, true);
		if (typeof(maxWorkers) == "undefined") {
			maxWorkers = hit.maxAssignments;
		}
		
		// This is where msbernst altered the code
		if (hit.done || hit.assignments.length >= maxWorkers) {
			verbosePrint("hit completed: " + hitId)
			return hit
		}
		else {
			return boundedStop(hitId, maximumWait, minWorkers, maxWorkers);
		}
	})
}

function boundedStop(hitId, maximumWait, minWorkers, maxWorkers) {
	var hit = mturk.getHIT(hitId, true);
	
	if (typeof(maxWorkers) == "undefined") {
		maxWorkers = hit.maxAssignments;
	}
	
	if (hit.done) {
		// we're done, and there's no point waiting any more! nothing more will come
		return hit;
	}
	print('creation time: ' + hit.creationTime);
	var remainingTime = maximumWait - (new Date() - hit.creationTime);
	if (remainingTime > 0 && hit.assignments.length < maxWorkers) {
		verbosePrint("waiting " + remainingTime/1000 + " seconds longer for hit: " + hitId);
		stop();
	}
	else {
		if (hit.assignments.length < minWorkers) {
			verbosePrint("time expired but still waiting for " + minWorkers + " workers: " + hitId);
			stop();		
		}
		else {
			verbosePrint("no longer waiting for hit: " + hitId);
			return hit;
		}
	}
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

Array.max = function( array ){
    return Math.max.apply( Math, array );
};

Array.min = function( array ){
    return Math.min.apply( Math, array );
};

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

function writeCSVPayment(csv, hit, title, paragraph_index)
{
	var hit_data = mturk.getHIT(hit, true);
	for (var i=0; i<hit_data.assignments.length; i++) {
		var out = []
		out.push(title);
		out.push(i+1);
		out.push(hit_data.reward);
		out.push(paragraph_index);
		csv.write(out.join(",") + '\n');
	}
}

function writeCSVWait(csv, hit, title, paragraph_index)
{
	var hit_data = mturk.getHIT(hit, true);
	for (var i=0; i<hit_data.assignments.length; i++) {
		var result = hit_data.assignments[i];
		var elapsedTimeSeconds = (result.submitTime - result.acceptTime)/1000;
		var waitTimeSeconds = (result.acceptTime - hit_data.creationTime)/1000;
		
		var work_out = [title, i+1, "work", elapsedTimeSeconds, paragraph_index];
		csv.write(work_out.join(",") + '\n');
		var wait_out = [title, i+1, "wait", waitTimeSeconds, paragraph_index];
		csv.write(wait_out.join(",") + '\n');
	}	
}

function extendHit(theHit, buffer_redundancy) {
	var extendTime;
	extendTime = once(getExtensionTime);
	var d = new Date(extendTime);
	print(d.getHours() + ':' + d.getMinutes() + ':' + d.getSeconds());
	
	var timePassed = (Math.round(new Date().getTime() / 1000) - extendTime);
	print(timePassed);
	
	var wait_between_extensions = 60;	// 1 minute
	var remaining_wait_time = wait_between_extensions - timePassed;
	if (remaining_wait_time > 0) {
		print('must wait another ' + (remaining_wait_time) + ' seconds to extend again');
		stop();
	} else
	{
		mturk.extendHIT(theHit, buffer_redundancy, 120);	// if we got no agreement, get more people
	}
}

function getExtensionTime()
{
	return Math.round(new Date().getTime() / 1000);	// in unix ticks
}

function numVotes(vote) {
	var count = 0;
	for (var k in vote) {
		if (vote.hasOwnProperty(k)) {
		   count += vote[k];
		}
	}
	return count;
}

function getHITStartTime(hitId) {
	var hit = mturk.getHIT(hitId, true);
	return hit.creationTime;
}

function getHITEndTime(hitId) {
	var hit = mturk.getHIT(hitId, true);
	if (hit.assignments.length > 0) {
		return hit.assignments[hit.assignments.length-1].submitTime;
	}
	else {
		return -1;
	}
}

if (!Array.prototype.reduce)
{
  Array.prototype.reduce = function(fun /*, initial*/)
  {
    var len = this.length >>> 0;
    if (typeof fun != "function")
      throw new TypeError();

    // no value to return if no initial value and an empty array
    if (len == 0 && arguments.length == 1)
      throw new TypeError();

    var i = 0;
    if (arguments.length >= 2)
    {
      var rv = arguments[1];
    }
    else
    {
      do
      {
        if (i in this)
        {
          var rv = this[i++];
          break;
        }

        // if array contains no values, no initial value to return
        if (++i >= len)
          throw new TypeError();
      }
      while (true);
    }

    for (; i < len; i++)
    {
      if (i in this)
        rv = fun.call(null, rv, this[i], i, this);
    }

    return rv;
  };

function cancelAllHITs() {
	print ("cancelAllHITS() called!");
	foreach(database.query("return keys(ensure('__HITs'))"), 
		function (hit) 
		{
			var m = hit.match(/^(real|sandbox):(.*)$/)
			if (m)
			{
				setMode(m[1])
				mturk.deleteHITRaw(m[2])
				print ("hit_utils: The cancel was successful! Hit " + m[2]);
			} 
			else 
			{
				mturk.deleteHITRaw(hit)
				print ("hit_utils: The cancel was successful! Hit " + hit);
			}
		}
	)
	foreach(database.query("return keys(ensure('__S3_Objects'))"), 
		function (obj) 
		{
			s3.deleteObjectRaw(obj);
			print ("hit_utils: Deleted S3 object " + obj);
		}
	);
	}
}