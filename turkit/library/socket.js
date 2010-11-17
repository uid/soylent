/**
 * The socket class encapuslates how we communicate with outside applications.
 */

function Socket(jobType, host, port, timeout) {
    this.jobType = jobType;
    this.host = host;
    this.port = port;
    this.timeout = timeout;
    
    this.socket = null;
    this.socketOut = null;  // to be written
}
Socket.FIND_STAGE = "find";
Socket.FIX_STAGE = "fix";
Socket.VERIFY_STAGE = "verify";

Socket.prototype.connect = function () {}
Socket.prototype.close = function() {}

Socket.prototype.sendStatus = function(stage, hit, paragraphNum, patchNumber, totalPatches, buffer_redundancy) {
	var url = (javaTurKit.mode == "sandbox"
					? "https://workersandbox.mturk.com/mturk/preview?groupId="
					: "https://www.mturk.com/mturk/preview?groupId=")
			+ hit.hitTypeId;            
    
	var message = {
		stage: stage,
		numCompleted: hit.assignments.length,
        totalRequested: hit.maxAssignments,
        payment: hit.reward,
		paragraph: paragraphNum,
        hitURL: url,
        patchNumber: patchNumber,
        totalPatches: totalPatches
	};
    if (message.stage == Socket.FIND_STAGE) {
        message.totalRequested -= 2 * buffer_redundancy;
    } else if (message.stage == Socket.FIX_STAGE || message.stage == Socket.VERIFY_STAGE) {
        message.totalRequested -= buffer_redundancy;
    }
    
	this.sendMessage("status", message);
}

Socket.prototype.sendStageComplete = function(stage, paragraphNum, hit, patchNumber, totalPatches) {
	var message = {
		stage: stage,
		totalRequested: hit.assignments.length,
		payment: hit.reward,
		paragraph: paragraphNum,
		patchNumber: patchNumber,
		totalPatches: totalPatches
	}
	
	this.sendMessage("stageComplete", message);
}

Socket.prototype.sendException = function(exceptionCode, exceptionString) {
	var message = {
		exceptionCode: exceptionCode,
		exceptionString: exceptionString
	}
	
	this.sendMessage("exception", message);
}

Socket.prototype.sendMessage = function(messageType, message, urlLocation) {
	// Default behavior is to send to C#
	if (urlLocation == null) {
		urlLocation = "http://localhost:11000/";
	}

    message.job = soylentJob;
    message.__type__ = messageType;
    message.__jobType__ = this.jobType;
	message.__awsAccessKeyId__ = javaTurKit.awsAccessKeyID;
	message.__protocolVersion__ = 0.01;		// increment as the protocol changes
    
    var stringMessage = json(message);
	stringMessage = stringMessage.substring(1, stringMessage.length-1); // remove the { } encasing the JSON. C# hates that.
    print(stringMessage);
    
	try 
	{
		var url = new java.net.URL(urlLocation);
		var connection = url.openConnection();
		connection.setRequestMethod("GET");
		connection.setReadTimeout(15*1000);
		connection.setDoOutput(true);
		
		connection.connect();	
		var out = new java.io.OutputStreamWriter(connection.getOutputStream());
		out.write(stringMessage);
		out.close();
		
		// read the output from the server
		var reader = new java.io.BufferedReader(new java.io.InputStreamReader(connection.getInputStream()));
		var stringBuilder = new java.lang.StringBuilder();
		stringBuilder.append("response:\n");

		var line = null;
		while ((line = reader.readLine()) != null)
		{
			stringBuilder.append(line + "\n");
		}
		print(stringBuilder.toString());
	}
	catch (e) 
	{
        print("Caught socket failure: " + e.rhinoException);
    }
}