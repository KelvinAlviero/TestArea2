mergeInto(LibraryManager.library, {

    _onRequestSuccess: null,
    _onRequestError: null,

    RegisterRequestCallbacks: function(onSuccess, onError) {
        this._onRequestSuccess = onSuccess;
        this._onRequestError = onError;
    },

    SendChannelMessage: function(action, payload) {
        var actionStr = UTF8ToString(action);
        var payloadStr = UTF8ToString(payload);
        if (window.uniwebview) {
        window.uniwebview.send(actionStr, JSON.parse(payloadStr));
        } else {
        console.log("[WebGLBridge] send simulated — action:", actionStr);
        }
    },

    CallChannelMessage: function(action, payload) {
        var actionStr = UTF8ToString(action);
        var payloadStr = UTF8ToString(payload);
        if (window.uniwebview) {
        var result = window.uniwebview.call(actionStr, JSON.parse(payloadStr));
        var resultStr = JSON.stringify(result);
        var bufferSize = lengthBytesUTF8(resultStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(resultStr, buffer, bufferSize);
        return buffer;
        } else {
        console.log("[WebGLBridge] call simulated — action:", actionStr);
        return 0;
        }
    },

    RequestChannelMessage: function(action, payload, timeout) {
        var actionStr = UTF8ToString(action);
        var payloadStr = UTF8ToString(payload);
        var self = this;
        if (window.uniwebview) {
        window.uniwebview.request(actionStr, JSON.parse(payloadStr), timeout)
            .then(function(data) {
            if (self._onRequestSuccess) {
                var str = JSON.stringify(data);
                var bufSize = lengthBytesUTF8(str) + 1;
                var buf = _malloc(bufSize);
                stringToUTF8(str, buf, bufSize);
                dynCall_vi(self._onRequestSuccess, buf);
            }
            })
            .catch(function(err) {
            if (self._onRequestError) {
                var str = JSON.stringify(err);
                var bufSize = lengthBytesUTF8(str) + 1;
                var buf = _malloc(bufSize);
                stringToUTF8(str, buf, bufSize);
                dynCall_vi(self._onRequestError, buf);
            }
            });
        } else {
        console.log("[WebGLBridge] request simulated — action:", actionStr);
        }
    }
});