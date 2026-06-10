// 🤖 Safar AI Chatbot — Premium Frontend Controller
document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById("chatToggle");
    const chatPanel = document.getElementById("chatPanel");
    const sendBtn = document.getElementById("chat-send");
    const inputField = document.getElementById("chat-input");
    const chatHistory = document.getElementById("chat-history");
    const typingIndicator = document.getElementById("typingIndicator");

    // Toggle chat panel open/close
    if (toggleBtn && chatPanel) {
        toggleBtn.addEventListener("click", function () {
            const isOpen = chatPanel.classList.toggle("open");
            toggleBtn.classList.toggle("active", isOpen);
            toggleBtn.textContent = isOpen ? "✕" : "💬";
            if (isOpen) {
                setTimeout(() => inputField?.focus(), 350);
            }
        });
    }

    // Send message handler
    if (sendBtn && inputField && chatHistory) {
        async function sendMessage() {
            const message = inputField.value.trim();
            if (!message) return;

            // 1. Display User Message Bubble
            const userBubble = document.createElement("div");
            userBubble.className = "chat-bubble user";
            userBubble.textContent = message;
            chatHistory.appendChild(userBubble);
            inputField.value = "";
            chatHistory.scrollTop = chatHistory.scrollHeight;

            // 2. Show typing indicator
            if (typingIndicator) typingIndicator.classList.add("visible");
            chatHistory.scrollTop = chatHistory.scrollHeight;

            // 3. Call the AI Backend
            try {
                const response = await fetch('/api/chat', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ message: message })
                });

                const data = await response.json();

                // 4. Hide typing indicator
                if (typingIndicator) typingIndicator.classList.remove("visible");

                // 5. Display AI Response Bubble
                const aiBubble = document.createElement("div");
                aiBubble.className = "chat-bubble ai";
                aiBubble.textContent = data.reply;
                chatHistory.appendChild(aiBubble);
                chatHistory.scrollTop = chatHistory.scrollHeight;

            } catch (error) {
                if (typingIndicator) typingIndicator.classList.remove("visible");
                
                const errorBubble = document.createElement("div");
                errorBubble.className = "chat-bubble ai";
                errorBubble.textContent = "⚠️ Connection error. Please try again.";
                chatHistory.appendChild(errorBubble);
                chatHistory.scrollTop = chatHistory.scrollHeight;
                console.error("Chatbot Error:", error);
            }
        }

        sendBtn.addEventListener("click", sendMessage);

        // Allow pressing 'Enter' to send
        inputField.addEventListener("keypress", function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                sendMessage();
            }
        });
    }
});