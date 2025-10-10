mergeInto(LibraryManager.library, {
  GetUrlParam: function (str) {
    var param = UTF8ToString(str);
    console.log("parma: " + param);
    var searchParams = new URLSearchParams(window.location.search);
    var result = searchParams.get(param);

    // Return null if not found
    if (result == null) {
      result = "";
    }

    // Allocate memory and write the result string
    var bufferSize = lengthBytesUTF8(result) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(result, buffer, bufferSize);
    return buffer;
  },
  RequestMotionPermission: function () {
    if (
      typeof DeviceMotionEvent !== "undefined" &&
      typeof DeviceMotionEvent.requestPermission === "function"
    ) {
      DeviceMotionEvent.requestPermission()
        .then((response) => {
          if (response == "granted") {
            window.addEventListener("devicemotion", (e) => {
              window.motionX = e.accelerationIncludingGravity.x;
              window.motionY = e.accelerationIncludingGravity.y;
              window.motionZ = e.accelerationIncludingGravity.z;
            });
          }
        })
        .catch(console.error);
    }
  },
  GetMotionY: function () {
    return window.motionX || 0;
  },
});
