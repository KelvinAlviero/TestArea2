mergeInto(LibraryManager.library, {

    RegisterRequestCallbacks: function (onSuccess, onError) {
        window.__uwvOnSuccess = onSuccess;
        window.__uwvOnError = onError;
    },

    SendChannelMessage: function (actionPtr, payloadPtr) {
        var action = UTF8ToString(actionPtr);
        var payloadStr = UTF8ToString(payloadPtr);
        var data = payloadStr === "null" ? null : JSON.parse(payloadStr);

        if (!window.uniwebview) {
            console.log("[WebGLBridge] send simulated", action, data);
            return;
        }
        window.uniwebview.send(action, data);
    },

    CallChannelMessage: function (actionPtr, payloadPtr) {
        var action = UTF8ToString(actionPtr);
        var payloadStr = UTF8ToString(payloadPtr);
        var data = payloadStr === "null" ? null : JSON.parse(payloadStr);

        if (!window.uniwebview) {
            console.log("[WebGLBridge] call simulated", action, data);
            return 0;
        }

        var result = window.uniwebview.call(action, data);
        var resultStr = JSON.stringify(result ?? null);
        var bufferSize = lengthBytesUTF8(resultStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(resultStr, buffer, bufferSize);
        return buffer;
    },

    RequestChannelMessage: function (actionPtr, payloadPtr, timeoutMs) {
        var action = UTF8ToString(actionPtr);
        var payloadStr = UTF8ToString(payloadPtr);
        var data = payloadStr === "null" ? null : JSON.parse(payloadStr);

        function invoke(fnPtr, text) {
            if (!fnPtr) return;
            var len = lengthBytesUTF8(text) + 1;
            var ptr = _malloc(len);
            stringToUTF8(text, ptr, len);
            {{{ makeDynCall('vi', 'fnPtr') }}}(ptr);
            _free(ptr);
        }

        if (!window.uniwebview || !window.uniwebview.request) {
            invoke(window.__uwvOnError, "uniwebview.request not available");
            return;
        }

        var settled = false;
        var timer = setTimeout(function () {
            if (settled) return;
            settled = true;
            invoke(window.__uwvOnError, "Timeout after " + timeoutMs + "ms");
        }, timeoutMs);

        window.uniwebview.request(action, data)
            .then(function (result) {
                if (settled) return;
                settled = true;
                clearTimeout(timer);
                var str = typeof result === "string" ? result : JSON.stringify(result ?? {});
                invoke(window.__uwvOnSuccess, str);
            })
            .catch(function (err) {
                if (settled) return;
                settled = true;
                clearTimeout(timer);
                var str = typeof err === "string" ? err : JSON.stringify(err ?? "Request failed");
                invoke(window.__uwvOnError, str);
            });
    }
});
