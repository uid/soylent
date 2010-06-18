// this variable now gets passed from c#
//var sentence_separator = " ";

function getParagraph(sentences) {
	return sentences.join(sentence_separator);
}

// Classes
function Patch(start, end, sentences) {
	this.start = start;
	this.end = end;
	this.sentences = sentences;
}

/*
 * Start and end bounds on the sentences the Patch spans
 */
Patch.prototype.sentenceRange = function() {
	var startSentence = null;
	var endSentence = null;
	var currentText = null;
	for (var i=0; i<this.sentences.length; i++) {
		currentText = this.sentences.slice(0, i+1).join(sentence_separator);
		if (currentText.length >= this.start && startSentence == null) {
			startSentence = i;
			startPosition = this.start - (currentText.length - this.sentences[i].length);
		}
		if (currentText.length >= this.start + (this.end - this.start)) {
			endSentence = i;
			endPosition = this.end - (currentText.length - this.sentences[i].length);
			break;
		}
	}	
	return {
		startPosition: startPosition,
		startSentence: startSentence,
		endPosition: endPosition,
		endSentence: endSentence
	};
}

Patch.prototype.highlightedSentence = function() {
	var range = this.sentenceRange();
	var sentence = ""
	for (var i=range.startSentence; i<=range.endSentence; i++) {
		if (i == range.startSentence) {	
			sentence = this.sentences[i].substring(0, range.startPosition);
			sentence += "<span style='background-color: yellow;'>"
			if (range.startSentence != range.endSentence) {
				sentence += this.sentences[i].substring(range.startPosition);
			}
		}
		if (i == range.endSentence) {
			var end_sentence_start;
			if (range.startSentence == range.endSentence) {
				end_sentence_start = range.startPosition
			}
			else {
				end_sentence_start = 0;
			}
			sentence += this.sentences[i].substring(end_sentence_start, range.endPosition);	// add everything
			sentence += "</span>";
			sentence += this.sentences[i].substring(range.endPosition);
		}
		if (i != range.startSentence && i != range.endSentence) {
			sentence += sentence_separator + this.sentences[i];
		}
	}
	return sentence;
};

Patch.prototype.highlightedParagraph = function() {
	var range = this.sentenceRange();
	
	var paragraph_sentences = []
	paragraph_sentences = paragraph_sentences.concat(this.sentences.slice(0, range.startSentence));
	paragraph_sentences = paragraph_sentences.concat(this.highlightedSentence());
	if (range.endSentence < this.sentences.length) {
		paragraph_sentences = paragraph_sentences.concat(this.sentences.slice(range.endSentence+1));
	}
	return paragraph_sentences.join(sentence_separator);
};

Patch.prototype.plaintextSentence = function() {
	var range = this.sentenceRange();
	return this.sentences.slice(range.startSentence, range.endSentence+1).join(sentence_separator);
};

Patch.prototype.getCutSentence = function() {
	var highlighted = this.highlightedSentence();
	return highlighted.replace(/<span.*?<\/span>\s?/ig, "");
}