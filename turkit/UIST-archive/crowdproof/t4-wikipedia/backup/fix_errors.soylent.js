var soylentJob = 1;

var paragraphs = [ 
	["Dandu Monara (Flying Peacock, Wooden Peacock), The Flying machine able to fly.",
	"The King Ravana (Sri Lanka) built it.",
	"Accorinding to hindu believes in Ramayanaya King Ravana used \"Dandu Monara\" for abduct queen Seetha from Rama.",
	"According to believes \"Dandu Monara\" landed at Werangatota, about 10 km from Mahiyangana.",
	"It is the hill station of Nuwara Eliya in central Sri Lanka." ] ];

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