var soylentJob = 1;

var paragraphs =	[
	[	"The graphical user interface (GUI) has proven both a successful and durable model for human-computer interaction.",
		"At the same time, the GUI approach falls short in many respects, particularly in embracing the rich interface modalities between people and the physical environments they inhabit.",
		"Systems exploring augmented reality and ubiquitous computing have begun to address this challenge.",
		"However, these efforts have often taken the form of exporting the GUI paradigm to more world-situated devices, falling short of much of the richness of physical-space interaction they seek to augment."
	],
	
	["In this paper, we present research developing \"Tangible User Interfaces\" (TUIs) as physical interfaces to digital information.", 
	"The metaDESK system is a graphically intensive system driven by interaction with graspable physical objects.", 
	"We introduce a prototype application driving an interaction with geographical space to illustrate our approach."
	],

	["The Tangible Bits vision paper introduced the metaDESK along with two companion platforms, the transBOARD and ambientROOM.",
	"Together, these platforms explore both graspable physical objects and ambient environmental displays as means for seamlessly coupling people, digital information, and the physical environment."
	],
	
	["The metaDESK system consists of the desk, a nearly-horizontal backprojected graphical surface; the active lens, an arm-mounted flat-panel display; the passive lens, an optically transparent surface through which the desk projects;",
	"These components are sensed by an array of optical, mechanical, and electromagnetic field sensors."
	],

	["Our research with the metaDESK system focuses on the use of tangible objects as driving elements of human-computer interaction.",
	"In particular, we are interested in pushing back from the GUI into the real world, physically instantiating many of the metaphorical devices the GUI has popularized.",
	"Simultaneously, we have attempted to push forward from the unaugmented physical world, inheriting from the richness of various historical instruments and devices often \"obsoleted\" by the advent of the computer."
	],

	["We more broadly explore physical affordances within TUI design.",
	"Our active lens looks, acts, and is manipulated like a jeweler's magnifying lens.",
	"In this way, the active lens suggests and supports a user's natural expectations from the device."
	],

	["We present our design approach making user interfaces tangible.",
	"The operating scenario of the Tangible Geospace prototype is then presented.",
	"This is followed by a description of the metaDESK implementation.",
	"Interaction issues encountered with the prototype are then discussed, followed by future work and conclusions."
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

main();

if (socket != null) {
	try {
		socket.close();
	} catch (e) {
		print(e.rhinoException);
	}
}