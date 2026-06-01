window.chatRecorder = (() => {
    let mediaRecorder;
    let chunks = [];

    return {
        start: async () => {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            chunks = [];
            mediaRecorder = new MediaRecorder(stream);
            mediaRecorder.ondataavailable = (event) => {
                if (event.data && event.data.size > 0) {
                    chunks.push(event.data);
                }
            };
            mediaRecorder.start();
        },
        stop: async () => {
            if (!mediaRecorder) {
                return null;
            }

            return await new Promise((resolve) => {
                mediaRecorder.onstop = async () => {
                    const blob = new Blob(chunks, { type: mediaRecorder.mimeType || "audio/webm" });
                    const buffer = await blob.arrayBuffer();
                    const bytes = new Uint8Array(buffer);
                    let binary = "";
                    for (let i = 0; i < bytes.byteLength; i++) {
                        binary += String.fromCharCode(bytes[i]);
                    }

                    mediaRecorder.stream.getTracks().forEach((track) => track.stop());

                    resolve({
                        fileName: "voice-message.webm",
                        contentType: blob.type || "audio/webm",
                        base64: btoa(binary)
                    });
                };

                mediaRecorder.stop();
            });
        }
    };
})();
