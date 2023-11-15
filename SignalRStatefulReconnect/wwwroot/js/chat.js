(() => {
    const connectButton = document.getElementById('connect');
    const disconnectButton = document.getElementById('disconnect');
    const autoReconnectCheckbox = document.getElementById('auto-reconnect');
    const statefulReconnectCheckbox = document.getElementById('stateful-reconnect');
    const sendButton = document.getElementById('send-button');
    const sendForm = document.getElementById('send-form');
    const messageInput = document.getElementById('message-input');
    const messageList = document.getElementById('message-list');
    let name, connection;

    function addLine(line, color) {
        var child = document.createElement('li');
        if (color) {
            child.style.color = color;
        }
        child.innerText = line;
        messageList.appendChild(child);
    }

    function updateButtonState() {
        let connected = connection?.state === signalR.HubConnectionState.Connected;
        connectButton.disabled = connected;
        disconnectButton.disabled = !connected;
        sendButton.disabled = !connected;
    }

    async function connect() {
        name = document.getElementById("display-name").value;
        if (name === "") {
            alert("Please enter a valid name");
            return;
        }

        var connectionBuilder = new signalR.HubConnectionBuilder()
            .withUrl(`chat?name=${name}`)
            .configureLogging("trace")
            .withServerTimeout(5 * 60 * 1000);

        if (autoReconnectCheckbox.checked) {
            connectionBuilder.withAutomaticReconnect();
        }
        if (statefulReconnectCheckbox.checked) {
            connectionBuilder.withStatefulReconnect();
        }

        connection = connectionBuilder.build();

        connection.on('Send', addLine);

        connection.onclose(error => {
            if (error) {
                addLine(`Connection closed with error: ${error}`, 'red');
            }
            else {
                addLine('Disconnected', 'green');
            }
            updateButtonState();
        });

        connection.onreconnecting(error => {
            addLine(`Connection reconnecting: ${error}`, 'orange');
        });

        connection.onreconnected(() => {
            addLine('Connection reconnected!', 'green');
        });

        try {
            await connection.start();
            updateButtonState();
            addLine('Connected successfully', 'green');
        } catch (e) {
            addLine(e, 'red');
        }
    }

    connectButton.addEventListener('click', connect);
    disconnectButton.addEventListener('click', () => connection?.stop());
    sendForm.addEventListener('submit', event => {
        if (connection?.state === signalR.HubConnectionState.Connected) {
            connection.invoke('SendAll', name, messageInput.value);
            sendForm.reset();
        }
        event.preventDefault();
    });

    updateButtonState();
})();
