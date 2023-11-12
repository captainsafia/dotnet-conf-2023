(function () {
    let name, connection, isConnected;
    let connectButton = document.getElementById('connect');
    let disconnectButton = document.getElementById('disconnect');
    let autoReconnectCheckbox = document.getElementById('auto-reconnect');
    let statefulReconnectCheckbox = document.getElementById('stateful-reconnect');
    let sendButton = document.getElementById('send-button');
    let sendForm = document.getElementById('send-form');
    let messageInput = document.getElementById('message-input');
    let messageList = document.getElementById('message-list');

    function addLine(line, color) {
        var child = document.createElement('li');
        if (color) {
            child.style.color = color;
        }
        child.innerText = line;
        messageList.appendChild(child);
    }

    function updateButtonState(connected) {
        isConnected = connected;
        connectButton.disabled = connected;
        disconnectButton.disabled = !connected;
        sendButton.disabled = !connected;
    }

    updateButtonState(false);

    sendForm.addEventListener('submit', function (event) {
        if (isConnected) {
            connection.invoke('SendAll', name, messageInput.value);
            sendForm.reset();
        }
        event.preventDefault();
    });

    connectButton.addEventListener('click', async function () {
        name = document.getElementById("display-name").value
        if (name === "") {
            alert("Please enter a valid name");
            return;
        }

        hubRoute = `chat?name=${name}`;
        console.log(`http://${document.location.host}/chat`);

        var connectionBuilder = new signalR.HubConnectionBuilder()
            .configureLogging("trace")
            .withUrl(hubRoute);

        if (autoReconnectCheckbox.checked) {
            connectionBuilder.withAutomaticReconnect();
        }
        if (statefulReconnectCheckbox.checked) {
            connectionBuilder.withStatefulReconnect();
        }

        connection = connectionBuilder.build();

        connection.on('Send', addLine);

        connection.onclose(function (e) {
            if (e) {
                addLine(`Connection closed with error: ${e}`, 'red');
            }
            else {
                addLine('Disconnected', 'green');
            }
            updateButtonState(false);
        });

        connection.onreconnecting(function (e) {
            addLine(`Connection reconnecting: ${e}`, 'orange');
        });

        connection.onreconnected(function () {
            addLine('Connection reconnected!', 'green');
        });

        try {
            await connection.start();
            updateButtonState(true);
            addLine('Connected successfully', 'green');
        } catch (e) {
            updateButtonState(false);
            addLine(e, 'red');
        }
    });

    disconnectButton.addEventListener('click', async function () {
        await connection.stop();
        updateButtonState(false);
    });
})();
