// Ensure the browser has permissions to access the camera and microphone. These permissions are crucial for WebRTC applications.
navigator.mediaDevices.getUserMedia({ video: true, audio: true })
    .then(stream => {
        console.log('Media stream acquired:', stream);
    })
    .catch(error => {
        console.error('Error accessing media devices:', error);
    });