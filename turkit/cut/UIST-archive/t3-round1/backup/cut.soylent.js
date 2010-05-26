var soylentJob = 1;

var paragraphs =	[
	[	"Too often, even the best information retrieval tools cannot help us find what we are seeking, because the information we want was never entered.",
		"This can happen for many reasons.",
		"Sometimes, we simply do not recognize that the information might be needed later.",
		"At other times, the perceived cost to launch and navigate through multiple applications to capture the information seems too high for the currently perceived value of the information.",
		"Lastly, our strong desire to record some information can be stymied by the fact that there is no natural place for it---no place where we have confidence that we will be able to find it when we need it, or, similarly, no native application that may be associated with the particular kind of data being entered."
	],
	
	[	"Many of these problems vanish if we turn to a much older recording technology---text.",
		"Recording a fragment of text simply requires picking up a pen or typing at a keyboard.",
		"When we enter text, each (pen or key) stroke is being used to record the actual information we care about---none is wasted on application navigation or configuration.",
		"The linear structure of text means there's always an obvious place to put anything---at the end.",
		"And the free form of text means we can record anything we want to about anything, without worrying whether it fits some application schema or should be split over multiple applications.",
		"All of this means that we have to do less to record text, which makes it more efficient and also less of an interruption and distraction than using a complex application."
	],

	[	"While text is an outstanding solution for recording information, its weakness lies in retrieval.",
		"Text's fixed linear form reduces us to scanning through it for information we need.",
		"Even with electronic text, the lack of structure means we cannot filter or sort by various properties of the information.",
		"When we aren't sure what we want, a blank text search box offers few cues to help us construct an appropriate query.",
		"The shorthand we use to record information in a given context can make it incomprehensible when we return to it later without that context.",
		"And only the text we explicitly enter is recorded, without any of the related information that might be known to a sophisticated application."
	],

	["In this paper we argue that it is possible and desirable to combine the easy input affordances of text with the powerful retrieval and visualization capabilities of graphical applications.",
	"We present WenSo, a tool that uses lightweight text input to capture richly structured information for later retrieval and navigation in a graphical environment.",
	"WenSo provides: \n " +
	"- entry of information by typing arbitrary scraps of text (with all the text-input benefits mentioned above) \n"  +
	"- inclusion of structured information in the text through a natural and extensible \"pidgin\" \n" +
	"- extraction of structure through lightweight recognition of entities and relationships between them \n" +
	"- association of automatically-measured context with the information being recorded \n" +
	"- search and faceted browsing based on tags, entities, and relations for finding relevant text scraps \n" +
	"- automatic routing of relevant pieces of the entered information to structured applications such as calendar, address book, and web browser so that it can be retrieved and visualized using those domain-specific tools."
	],

	["In order to deliver these interactions, we had to solve several key problems: capturing structure from text not entered in a form, modeling capture of desktop state for appropriate association with a scrap, and integration of captured data for use with existing applications.", 
	"In the following sections we present the related work that informs our approach, describe the interaction design and describe in our solutions for the key system implementation challenges.", 
	"We then discuss the future research opportunities both for extending the WenSo platform, but most immediately, for using the platform to determine what happens in terms of information scrap entry and reuse behaviour once these new affordances have been provided."
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