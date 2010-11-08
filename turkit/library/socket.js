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
/*
Socket.prototype.connect = function() {
    this.socket = new java.net.Socket();
    var endpoint = new java.net.InetSocketAddress(this.host, this.port);

    if (endpoint.isUnresolved()) {
        print("Failure :" + endpoint.toString());
    }
    else {
        try {
				this.socket.setSendBufferSize(100000);
                this.socket.connect(endpoint, this.timeout);
                print("Success: " + endpoint.toString());
				print("Output buffer size: " + this.socket.getSendBufferSize()); 
                this.socketOut = new java.io.PrintWriter(this.socket.getOutputStream(), true);
        } catch (e) {
            print("Failure: " + e.rhinoException);
        }
    }
}
*/

Socket.prototype.close = function() {}
/*
Socket.prototype.close = function() {
    if (this.socket != null) {
        try {
            this.socket.close();
        } catch (e) {
            print(e.rhinoException);
        }
    }
}
*/

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

Socket.prototype.sendMessage = function(messageType, message) {
    message.job = soylentJob;
    message.__type__ = messageType;
    message.__jobType__ = this.jobType;
    
    var stringMessage = json(message);
	stringMessage = stringMessage.substring(1, stringMessage.length-1); // remove the { } encasing the JSON. C# hates that.
    print(stringMessage);
    
	try {
		var url = new java.net.URL("http://localhost:11000/");
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

		  var line = null;
		  while ((line = reader.readLine()) != null)
		  {
			stringBuilder.append(line + "\n");
		  }
		print(stringBuilder.toString());
	} catch(e) {
		print(e.rhinoException);
	}
	
	//connection.setRequestMethod("POST");
	//connection.setDoOutput(true);

	//var out = new java.io.OutputStreamWriter(connection.getOutputStream());
	//out.write("foo");
	//out.close();
	
	
	/*
    if (this.socketOut == null) {
		print("Not in socket mode, not writing.");
	} else {
    	this.socketOut.print(stringMessage);
		this.socketOut.flush();
    }
	*/
}