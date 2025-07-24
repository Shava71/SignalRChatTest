// Set variables
let localStream;
let remoteStream = new MediaStream();
const localVideo = document.getElementById("localVideo");
const remoteVideo = document.getElementById("remoteVideo");

// Set media stream
const constraints = {video: true, audio: true };
navigator.mediaDevices.getUserMedia(constraints)
.then(stream => {
    localVideo.srcObject = stream;
    localStream = stream;

    localStream.getTracks().forEach(track => {
        peerConnection.addTrack(track, localStream);
    });
}).catch(error => {
    console.error('Error accessing media devices:', error);
    alert("Please turn on video-audio stream");
});

// Create WebRTC connection
const peerConnection = new RTCPeerConnection({
    iceServers: [
        { urls: 'stun:stun.l.google.com:19302' } // you can also use other stun servers 
    ]
});

// Display the remote stream 
peerConnection.ontrack = event => {
    event.streams[0].getTracks().forEach(track => {
        remoteStream.addTrack(track);
    })
    remoteVideo.srcObject = remoteStream; // after add to html this element: <video id="remoteVideo" autoplay playsinline></video>
}

const SignalRConn = new signalR.HubConnectionBuilder()
    .withUrl("/voicechat")
    .withAutomaticReconnect([0, 2000, 10000, 30000, null])
    .configureLogging(signalR.LogLevel.Information)
    .build();

SignalRConn.start()
    .then(() => {
        console.log("SignalR connected");
    })
    .catch(err => {
        console.error("SignalR connection error", err);
    });

// Send offer via Signaling
peerConnection.onicecandidate = (event) => {
    if(event.candidate) {
        SignalRConn.invoke("SendSignal", JSON.stringify(event.candidate));
    }
}

// Send answer via Signaling
SignalRConn.on("ReceiveSignal", (username, message) => {
    const signal = JSON.parse(message);
    if(signal.type == "offer") {
        peerConnection.setRemoteDescription(new RTCSessionDescription(signal));
        peerConnection.createAnswer()
            .then(answer => {peerConnection.setLocalDescription(answer);})
            .then(() => {
                SignalRConn.invoke("SendSignal", JSON.stringify(peerConnection.localDescription))
            }).catch((err) => {console.log("error to send SDP",err)})
    }
    else if(signal.candidate) {
        peerConnection.addIceCandidate(new RTCIceCandidate(signal));
    }
})


function startCall(){
    peerConnection.createOffer()
        .then(offer => {peerConnection.setLocalDescription(offer);})
        .then(() => {
            SignalRConn.invoke("SendSignal", JSON.stringify(peerConnection.localDescription))
        }).catch((err) => {console.log("error to send offer",err)})
}
document.getElementById("startCallBtn").addEventListener("click", startCall);


