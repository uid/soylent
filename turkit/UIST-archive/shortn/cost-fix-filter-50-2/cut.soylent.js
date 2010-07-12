var soylentJob = 1;

var paragraphs =	[
	[	"The graphical user interface (GUI) has proven both a successful and durable model for human-computer interaction which has dominated the last decade of interface design.",
		"At the same time, the GUI approach falls short in many respects, particularly in embracing the rich interface modalities between people and the physical environments they inhabit.",
		"Systems exploring augmented reality and ubiquitous computing have begun to address this challenge.",
		"However, these efforts have often taken the form of exporting the GUI paradigm to more world-situated devices, falling short of much of the richness of physical-space interaction they seek to augment."
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
eval(read("../template/cut.js"));

find = false;
fix = true;
filter = true;
debug = false;
time_bounded = false;

var search_reward = 0.50;
var edit_reward = 0.50;
var verify_reward = 0.03;
var cuts = [		// from output elsewhere
				new Patch(188, 349, paragraphs[0]),
				new Patch(113, 169, paragraphs[0])
			];			

main();

if (socket != null) {
	try {
		socket.close();
	} catch (e) {
		print(e.rhinoException);
	}
}