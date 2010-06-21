var soylentJob = 1;

var paragraphs =	[
	[	"Lyne,\n" +
		"My name is Mark Bain and I'm the Web Site Adminstrator for the Mariners.",
		"I would be glad to help.",
		"Please pass this email and my phone numbers (281-555-555 hm. 281-555-5555 wk.) to your website person.",
		"I put alot of info in this email to help get started but if it is a little overwhelming, just call."
	],

	["Steve Burleigh created our web site last year and gave me alot of ideas.",
	"I found a web site called eTeamZ that hosts web sites for sports groups.",
	"Check out our new page: http://www.eteamz.com/swimmariners/"
	],

	["eTeamsZ is really easy to use and does not require much web site knowledge.",	
	"They do the formatting and you supply the info.",  
	"they can do some custome stuff as well.",
	"The best part, however, is that it is FREE."
	],

	[" eTeamZ, that's fine too.",
	"I've created some other web sites and should be able to answer some questions.",
	"If you have experience, it should be easy.",
	"Good luck and let me know what else you need."
	],

	["Oh, here are some tips on getting started with eTeamZ.",
	" Registration- http://www.eteamz.com/company/sites/register/",
	"The toughest part was coming up with a nickname and a user name for the websites.",
	"Seems that most of the users are into baseball and Mariners was taken.",
	"I think I used Mariners Swim Team for nickname and goMariners for the username because I kept hitting other name that were being used."
	],

	["WebSite- When I got to the \"build you web\" page, I chose Team Web Site.",
	"They also had options for Leagues and Orgs.",
	"Start populating things via the admin page and bring up another window to check the progress of what is actually displayed.",
	"PLUS - They have a premium service called PLUS that you pay for but I didn't see much use for it except you get rid of some of the advertising banners.",
	"They also show that you can use this site for registration and other things but we are keeping it simple."
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