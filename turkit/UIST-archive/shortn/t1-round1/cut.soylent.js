var soylentJob = 1;

var paragraphs =	[
	[	"Print publishers are in a tizzy over Apple's new iPad because they hope to finally be able to charge for their digital editions.",
		"But in order to get people to pay for their magazine and newspaper apps, they are going to have to offer something different that readers cannot get at the newsstand or on the open Web.",
		"We've already seen plenty of prototypes  from magazine publishers which include interactive graphics, photo slide shows, and embedded videos."
	],
	
	[	"But what should a magazine cover look like on the iPad?",
		"After all, the cover is still the gateway to the magazine.",
		"Theoretically, it will still be the first page people see, giving them hints of what's inside and enticing them to dive into the issue.",
		"One way these covers could change is that instead of simply repurposing the static photographs from the print edition, the background image itself could be some sort of video loop.",
		"Jesse Rosten, a photographer in California, created the video mockup below of what a cover of Sunset Magazine might look like on the iPad (see video below)."
	],
	
	[	"The video shows ocean waves gently lapping a beach as the title of the magazine and other typographical elements appear on the page almost like movie credits.", 
		"He points out that these kinds of videos will have to be shot in a vertical orientation rather than a horizontal landscape one.",
		"This is just a mockup Rosten came up with on his own, but the designers of these new magazine apps should take note.",
		"The only way people are going to pay for these apps is if they create new experiences for readers."
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