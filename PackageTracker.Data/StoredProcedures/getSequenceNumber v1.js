﻿// getSequences
function getSequenceNumber(siteName, sequenceType, maxSequence, startSequence) {
    var container = getContext().getCollection();
    var queryBySite = {
        'query': 'SELECT TOP 1 * FROM sequences s WHERE s.siteName = @siteName AND s.sequenceType = @sequenceType',
        'parameters': [{ 'name': '@siteName', 'value': siteName }, { 'name': '@sequenceType', 'value': sequenceType }]
    };

    // Query documents and take 1st item.
    var isAccepted = container.queryDocuments(container.getSelfLink(), queryBySite,
        function (err, sequence, responseOptions) {
            if (err) throw new Error("Error" + err.message);

            // Check the feed and if empty, set the body to 'no docs found', 
            // else take 1st element from feed
            if (!sequence || !sequence.length) {
                var response = getContext().getResponse();
                response.setBody('no docs found');
            }
            else {
                var currentSequence = sequence[0];
                incrementSequence(currentSequence, maxSequence, startSequence);
                var response = getContext().getResponse();
                response.setBody(currentSequence);
            }
        });

    if (!isAccepted) throw new Error('The query was not accepted by the server.');

    // update the sequence
    function incrementSequence(currentSequence, maxSequence, startSequence) {

        if (currentSequence.number > parseInt(maxSequence)) {
            currentSequence.number = parseInt(startSequence);
        }
        else {
            currentSequence.number += 1;
        }

        var accept = container.replaceDocument(currentSequence._self, currentSequence,
            function (err, itemReplaced) {
                if (err) throw "Unable to update sequence";
            });

        if (!accept) throw "Unable to update sequence";
    }
}