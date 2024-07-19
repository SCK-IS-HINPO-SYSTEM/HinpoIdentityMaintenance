function openModal(id, url) {
    $('#error_message').val('');
    // コントローラ名/メソッド名
    //const url = new URL(window.location.href) + 'Modal/ShowModal';
    //const url = "https://localhost:44351/HinpoIdentityMaintenance/modal/showmodal";
    //var url = new URL(window.location.href) + pagename;
    // パラメータ成形
    let form = new FormData();
    form.append('id', id);
    $.ajax({
        url: url,
        type: 'POST',
        processData: false,
        contentType: false,
        data: form
    }).done(function(data){
        const data_stringify = JSON.stringify(data);
        const data_json = JSON.parse(data_stringify);
        if(data_json.errorMessage != null) {
            $('#error_message').append(data_json.errorMessage);
        } else {
            // 親画面に置いておいたdivに、コントローラから受け取ったモーダルのcshtmlをセットする
            $('#modal_base').html(data);
            // モーダル表示
            $('#modal_base').find('.modal').modal('show');
        }
    });
}