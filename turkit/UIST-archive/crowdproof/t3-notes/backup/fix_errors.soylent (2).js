var soylentJob = 1;

var paragraphs = [ ["However, while GUI made using computers be more intuitive and easier to learn, it didn't let people be able to control computers efficiently.", "Masses only can use the software developed by software companies, unless they know how to write programs.", "In other words, if one who knows nothing about programming needs to click through 100 buttons to complete her job everyday, the only thing she can do is simply to click through those buttons by hand every time.", "But if she happens to be a computer programmer, there is a little chance that she can write a program to automate everything.", "Why is there only a little chance?", "In fact, each GUI application is a big black box, which usually have no outward interfaces for connecting to other programs.", "In other words, this truth builds a great wall between each GUI application so that people have difficulty in using computers efficiently.", "People still do much tedious and repetitive work in front of a computer."] ];

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