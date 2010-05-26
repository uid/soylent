var soylentJob = 1;
var paragraphs = [[""]];
/*
The specific soylentJob number and paragraphs array will be written by C#.  Here is an example:

var soylentJob = 1;

var paragraphs =	[
	[	"The graphical user interface (GUI) has proven both a successful and durable model for human-computer interaction which has dominated the last decade of interface design.",
		"At the same time, the GUI approach falls short in many respects, particularly in embracing the rich interface modalities between people and the physical environments they inhabit.",
		"Systems exploring augmented reality and ubiquitous computing have begun to address this challenge.",
		"However, these efforts have often taken the form of exporting the GUI paradigm to more world-situated devices, falling short of much of the richness of physical-space interaction they seek to augment."
	],
	
	["In this paper, we present research developing \"Tangible User Interfaces\" (TUIs) - user interfaces employing physical objects, instruments, surfaces, and spaces as physical interfaces to digital information.", 
	"In particular, we present the metaDESK system, a graphically intensive system driven by interaction with graspable physical objects.", 
	"In addition, we introduce a prototype application driving an interaction with geographical space, Tangible Geospace, to illustrate our approach."
	],

	["The metaDESK effort is part of the larger Tangible Bits project.",
	"The Tangible Bits vision paper introduced the metaDESK along with two companion platforms, the transBOARD and ambientROOM.",
	"Together, these platforms explore both graspable physical objects and ambient environmental displays as means for seamlessly coupling people, digital information, and the physical environment."
	],
	
	["The metaDESK system consists of several components: the desk, a nearly-horizontal backprojected graphical surface; the active lens, an arm-mounted flat-panel display; the passive lens, an optically transparent surface through which the desk projects; and an assortment of physical objects and instruments which are used on desk's surface.",
	"These components are sensed by an array of optical, mechanical, and electromagnetic field sensors."
	],

	["Our research with the metaDESK system focuses on the use of tangible objects - real physical entities which can be touched and grasped - as driving elements of human-computer interaction.",
	"In particular, we are interested in pushing back from the GUI into the real world, physically instantiating many of the metaphorical devices the GUI has popularized.",
	"Simultaneously, we have attempted to push forward from the unaugmented physical world, inheriting from the richness of various historical instruments and devices often \"obsoleted\" by the advent of the computer."
	],

	["In addition, we more broadly explore the use of physical affordances within TUI design.",
	"For example, our active lens is not only grounded in the metaphor of a jeweler's magnifying lens; it also looks, acts, and is manipulated like such a device.",
	"In this way, the active lens has a certain legibility of interface in that its affordances suggest and support user's natural expectations from the device."
	],

	["In the following sections, we present our design approach towards making user interfaces tangible.",
	"The operating scenario of the Tangible Geospace prototype is then presented.",
	"This is followed by a description of the metaDESK implementation, including display, sensor, and software architectures. Interaction issues encountered with the prototype are then discussed, followed by future work and conclusions."
	]

];
*/

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

var debug = false;
// imports
eval(read("../templates/shortn/shortn.js"));

var find = true;
var fix = true;
var filter = true;

main();

if (socket != null) {
	try {
		socket.close();
	} catch (e) {
		print(e.rhinoException);
	}
}
