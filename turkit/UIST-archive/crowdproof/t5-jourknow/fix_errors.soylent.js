var soylentJob = 1;

var paragraphs = [ 
	[
	"Many of these problems vanish if we turn to a much older recording technology---text.",
	"Recording a fragment of text simply requires picking up a pen or typing at a keyboard.",
    "When we enter text, each (pen or key) stroke is being used to record the actual information we care about---none is wasted on application navigation or configuration.",
	"The linear structure of text means there's always an obvious place to put anything---at the end.",
	"And the free form of text means we can record anything we want to about anything, without worrying whether it fits some application schema or should be split over multiple applications.",
	"All of this means that we have to do less to record text, which makes it more efficient and also less of an interruption and distraction than using a complex application."
	] 
];

if (typeof(soylentJob) != "undefined") {
	var host = "localhost";
	var port = 11000;
	var timeout = 2000; // seconds
	var socket = new java.net.Socket();
	var endpoint = new java.net.InetSocketAddress(host, port);
	var socketOut = null;

	if (endpoint.isUnresolved()) {
		print("Failure :" + endpoint.toString());
	}
	else {
		try {
				socket.connect(endpoint, timeout);
				print("Success: " + endpoint.toString());
				socketOut = new java.io.PrintWriter(socket.getOutputStream(), true);
		} catch (e) {
			print("Failure: " + e.rhinoException);
		}
	}
}
else {
	print("WARNING: unknown job. No socket communication");
	stop();
}

// imports
eval(read("../template/fix_errors.js"));

main();

if (socket != null) {
	try {
		socket.close();
	} catch (e) {
		print(e.rhinoException);
	}
}