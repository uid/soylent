toBlock = ["A521UZVFXBVWZ", "A2T3MP92LA6X7S"];

for (var i=0; i<toBlock.length; i++) {
	worker = toBlock[i];
	print(worker);
	print(new XML(javaTurKit.restRequest("BlockWorker", "WorkerId", worker, "Reason", "Repeated bad behavior on tasks, across multiple days and after warning + rejection.")));
}

toUnblock = ["A2RL28EX7BIT15"];
for (var i=0; i<toUnblock.length; i++) {
	worker = toUnblock[i];
	print(worker);
	print(new XML(javaTurKit.restRequest("UnblockWorker", "WorkerId", worker, "Reason", "Discussion with the worker.")));
}
