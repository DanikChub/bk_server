(() => {
    const chatInput = $(".chat-input");
    const chatInputContainer =  $(".chat-input-container");

    chatInputContainer.hide();

    $(document).ready(() => {
        $('#summernote2').summernote();
    });

    chatInput.on("focus", () => {
        chatInput.hide();
        chatInputContainer.show();
    });
})();