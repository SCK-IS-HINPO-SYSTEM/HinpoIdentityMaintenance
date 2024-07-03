function openModal(id, url) {
    $('#error_message').val('');
    // �R���g���[����/���\�b�h��
    //const url = new URL(window.location.href) + 'Modal/ShowModal';
    //const url = "https://localhost:44351/HinpoIdentityMaintenance/modal/showmodal";
    //var url = new URL(window.location.href) + pagename;
    // �p�����[�^���`
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
            // �e��ʂɒu���Ă�����div�ɁA�R���g���[������󂯎�������[�_����cshtml���Z�b�g����
            $('#modal_base').html(data);
            // ���[�_���\��
            $('#modal_base').find('.modal').modal('show');
        }
    });
}