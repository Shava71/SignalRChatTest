const SignalRConn = new signalR.HubConnectionBuilder()
    .withUrl("/voicechat")
    .withAutomaticReconnect([0, 2000, 10000, 30000, null])
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Send offer via Signaling
peerConnection.onicecandidate = (event) => {
    if(event.candidate) {
        SignalRConn.invoke("SendSignal", JSON.stringify(event.candidate));
    }
}

SignalRConn.start();

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

