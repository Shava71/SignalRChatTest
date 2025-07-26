// Set variables
let localStream;
let remoteStream = new MediaStream();

// Video HTML-elements
const localVideo = document.getElementById("localVideo");
const remoteVideo = document.getElementById("remoteVideo");

// Create WebRTC-peer connection
const peerConnection = new RTCPeerConnection({
    iceServers: [
        { urls: 'stun:stun.l.google.com:19302' } // you can also use other stun servers 
    ]
});

// Pending ICE candidates buffer
let pendingCandidates = [];




// Display the remote stream after receive remote track
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
        SignalRConn.invoke("SendSignal", JSON.stringify({candidate: event.candidate}))
            .catch(err => {console.error("Error while sending ICE",err)});
    }
}

// Send answer via Signaling
SignalRConn.on("ReceiveSignal", async (username, message) => {
    if(!message) {return;}
    let signal;
    try {
        signal = JSON.parse(message);
    }
    catch (error) {
        console.error("Invalid signal JSON", error);
        return;
    }

    if(signal.type === "offer") {
        await peerConnection.setRemoteDescription(new RTCSessionDescription(signal));
        const answer = await peerConnection.createAnswer();
        await peerConnection.setLocalDescription(answer);
        await SignalRConn.invoke("SendSignal", JSON.stringify(peerConnection.localDescription))
        // peerConnection.createAnswer()
        //     .then(answer => {peerConnection.setLocalDescription(answer);})
        //     .then(() => {
        //         SignalRConn.invoke("SendSignal", JSON.stringify(peerConnection.localDescription))
        //     }).catch((err) => {console.log("error to send SDP",err)})
    }
    else if (signal.type === "answer") {
        await peerConnection.setRemoteDescription(new RTCSessionDescription(signal));
        while(pendingCandidates.length > 0) {
            await peerConnection.addIceCandidate(new RTCIceCandidate(pendingCandidates.shift()));
        }
    }
    else if(signal.candidate) {
        if(peerConnection.remoteDescription) {
            await peerConnection.addIceCandidate(new RTCIceCandidate(signal.candidate));
        }
        else{
            console.log("Ice candidate pushed to queue until remoteDesccription is set");
            pendingCandidates.push(signal.candidate);
        }
    }
})

SignalRConn.on("IncomingCall", function(fromUserName, fromUserId) {
    document.getElementById("callFromInput").textContent = fromUserName;
    document.getElementById("CallModal").style.display = "block";
    
    document.getElementById("acceptCall").addEventListener("click", async () => {
        SignalRConn.invoke("AcceptCall", fromUserId)
            .catch(err => {console.error("Error while accepting call",err)});
        document.getElementById("CallModal").style.display = "none";
        
        document.getElementById("WebRTCVoiceChatModal").style.display = "block";
        document.getElementById("callingToInput").textContent = fromUserName;
        document.getElementById("callingToInputId").value = fromUserId;

        await startRTC();
    })

    document.getElementById("declineCall").addEventListener("click", async () => {
        SignalRConn.invoke("DeclineCall", fromUserId)
            .catch(err => {console.error("Error while decline call",err)});
        document.getElementById("CallModal").style.display = "none";
    })
})

SignalRConn.on("CallAccepted", function () {
    console.log("Пользователь принял звонок.");
    startCall(); 
});

SignalRConn.on("CallDeclined", function () {
    console.log("Пользователь отклонил звонок.");
    alert("Звонок отклонён");
    document.getElementById("WebRTCVoiceChatModal").style.display = "none";

});

// GET and SET media stream
const constraints = {video: true, audio: true };
async function GetStream(){
    navigator.mediaDevices.getUserMedia(constraints)
        .then(stream => {
            localVideo.srcObject = stream;
            localStream = stream;

            localStream.getTracks().forEach(track => {
                peerConnection.addTrack(track, localStream);
            });
        }).catch(error => {
        console.error('Error accessing media devices:', error);
        // alert("Please turn on video-audio stream");
    });
}

// async function startCall(id){
//     SignalRConn.invoke("CallUser",id)
//         .then(()=>console.log("Starting call...",id))
//         .catch(err=>{console.error("Error while starting call...",err)});
//
//     document.getElementById("callingToInput").value = user.username;
//     document.getElementById("callingToInputId").value = user.id;
//     document.getElementById("WebRTCVoiceChatModal").style.display = "block";
// }
async function startRTC(){
    await GetStream();
    peerConnection.createOffer()
        .then(offer => {peerConnection.setLocalDescription(offer);})
        .then(() => {
            SignalRConn.invoke("SendSignal", JSON.stringify(peerConnection.localDescription))
        }).catch((err) => {console.log("error to send offer",err)})
}
// document.getElementById("startCallBtn").addEventListener("click", startCall);


