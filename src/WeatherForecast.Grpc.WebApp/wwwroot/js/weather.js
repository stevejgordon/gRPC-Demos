"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/weatherHub")
    .build();

document.getElementById("streamButton").addEventListener("click", async (event) => {
    try {
        connection.stream("WeatherStream")
            .subscribe({
                next: (item) => {
                    var li = document.createElement("li");
                    li.textContent = item;
                    document.getElementById("messagesList").appendChild(li);
                },
                complete: () => {
                    var li = document.createElement("li");
                    li.textContent = "Stream completed";
                    document.getElementById("messagesList").appendChild(li);
                },
                error: (err) => {
                    var li = document.createElement("li");
                    li.textContent = err;
                    document.getElementById("messagesList").appendChild(li);
                }
            });
    } catch (e) {
        console.error(e.toString());
    }
    event.preventDefault();
});

(async () => {
    try {
        await connection.start();
    } catch (e) {
        console.error(e.toString());
    }
})();
