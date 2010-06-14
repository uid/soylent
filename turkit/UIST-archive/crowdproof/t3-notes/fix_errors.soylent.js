var soylentJob = 1;

var paragraphs = [ 
	["Panel: NoSQL in the Cloud\n" +
	"Blah blah blah -- argument about whether there should be a standard \"nosql storage\" API to protect developers storing their stuff in proprietary services in the cloud.",
	"Probably unrealistic.",
	"To protect yourself, use an open software offering, and self-host or go with hosting solution that uses open offering.",
	],

	["Interesting discussion on disaster recovery.",
	"Since you've outsourced operations to the cloud, should you just trust the provider w/ diaster recovery.",
	"People kept talking about busses driving through datacenters or fires happening.",
	"What about the simpler problem: a developer drops your entire DB. Need to protect w/ backups no matter where you host."
	] ];

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