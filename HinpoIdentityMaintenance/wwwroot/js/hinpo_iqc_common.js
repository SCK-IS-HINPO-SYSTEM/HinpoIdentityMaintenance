/******************************************************************************************************
 *  入力項目制限
 ******************************************************************************************************/
$(document).ready(function () {
    // 数値項目のマイナスを許可しない
    $(".no-minus").keydown(function (e) {
        if (e.key === '-' || e.keycode === 189) e.preventDefault();
    }); 

    // セレクタに対する入力制限セット《整数用》
    function applyInputRestrictions(selector, maxLength) {
        $(selector).on('keydown input paste', function (e) {

            // 数値入力でeを防ぐ
            if (e.key === 'e' || e.keycode === 69) e.preventDefault();

            // 整数n桁入力制限 最大桁数を超える入力を防ぐ backとdeleteは許可 
            if (!isInputAllowed(e, maxLength, $(this).val())) { e.preventDefault(); } 

            // input イベント 要素の値が変更された時に発火する
            if (e.type === 'input') { 
                // 入力文字数が最大文字数を超えたら
                if (this.value.length > maxLength) {
                    // 例:1234567が入力されたら123456になる
                    this.value = this.value.slice(0, maxLength);
                }
            } 

            // paste イベント 貼り付け時に発火する
            if (e.type === 'paste') {
                // DOMに反映されるのを待つ
                setTimeout(() => {
                    // 現在の値を取得
                    var value = $(this).val();
                    // 許容値の正規表現 1～n桁までの数値に制限
                    var regex = new RegExp('^\\d{1,' + maxLength + '}$');
                    // 適合チェック
                    if (!regex.test(value)) {
                        // していない場合入力フィールドを空にする
                        $(this).val('');
                    }
                }, 0);
            } 
        });
    }  

    // セレクタに対する入力制限セット《英数字用》
    function applyInputAlphaNumeric(selector) {
        $(selector).on('keydown input paste', function (e) {
             
            // 英数字のみ入力可
            if (!isAlphaNumeric(e)) { e.preventDefault(); } 

            // input イベント 英数字以外が入力されたらその入力を削除
            if (e.type === 'input') { 
                this.value = this.value.replace(/[^a-zA-Z0-9]/g,''); 
            } 

            // paste イベント 貼り付け時に発火する
            if (e.type === 'paste') {
                // DOMに反映されるのを待つ
                setTimeout(() => {
                    // 現在の値を取得
                    var value = $(this).val(); 
                    // 英数字に制限
                    var regex = new RegExp(/[^a-zA-Z0-9]/g);
                    // 許可文字列以外削除
                    var newValue = value.replace(regex, '');
                    $(this).val(newValue);
                }, 0);
            }
        });
    }  

    // 文字数制限
    function maxLengthString(selector, maxLength) {
        $(selector).on('keydown input paste', function (e) {
            var currentLength = $(this).val().length; 
            if (currentLength >= maxLength) { 
                $(this).val($(this).val().slice(0, maxLength));
            }
        });
    }  
    applyInputAlphaNumeric('.input-alpha-numeric'); 
    applyInputRestrictions('.int-6', 6);    // .int-6 セレクタに対して6文字まで入力可
    // applyInputRestrictions('.int-7', 7); // .int-7 セレクタに対して7文字まで入力可
    maxLengthString('.rejectMessage', 512); // 却下メッセージ 文字数制限 512文字 半角１文字全角１文字改行１カウント
    maxLengthString('.cancelMessage', 512); // 取消メッセージ 文字数制限 512文字 半角１文字全角１文字改行１カウント
});
// URLのクエリ文字列をオブジェクトに変換
function HinpoIdentityMaintenanceGetQueryParams() {
    let params = {};
    let queryString = window.location.search.substring(1);
    let pairs = queryString.split("&");
    pairs.forEach(function (pairs) {
        let [key, value] = pairs.split("=");
        if (key && value) {
            // パラメータ名は小文字に統一
            params[decodeURIComponent(key).toLowerCase()] = decodeURIComponent(value.replace(/\+/g, " "));
        }
    });
    return params;
}

function windowOpenDisableLinks() {
    // ページ上のあるクラスを除くAタグに対して・・・
    var links = document.querySelectorAll('a:not(.window-open-close-button):not(.a-upload)');

    // 不具合連絡書とロットアウトのモーダレス表示の時のみヘッダタイトルのリンクを無効にする
    document.querySelectorAll('._layout-sckhinpo-header-title').forEach(link => {
        link.addEventListener('click', function (event) {
            event.preventDefault();
        });
        link.style.pointerEvents = 'none';
        link.style.textDecoration = 'none'; 
    });

    // window-open-close-button（権限エラーページの閉じるボタン）と
    // a-upload（アップロードリンク）以外は
    // 右クリックメニューとリンククリックを無効にする
    links.forEach(function (link) {
        // 右クリックのメニューを出させない
        link.addEventListener('contextmenu', function (e) {
            e.preventDefault();
            return false; 
        });
        // リンククリックさせない
        link.addEventListener('click', function (e) {
            e.preventDefault();
        });
        // リンククリックさせない
        link.addEventListener('onclick', function (e) {
            e.preventDefault();
        });
    }); 

    // a-upload（アップロードリンク）は
    // 右クリックメニューを無効にする
    var uploadlinks = document.querySelectorAll('a.a-upload');
    uploadlinks.forEach(function (link) {
        // 右クリックのメニューを出させない
        link.addEventListener('contextmenu', function (e) {
            e.preventDefault();
            return false;
        }); 
    });
}

// 入力制限判定ロジック(数値のみ)
function isInputAllowed(e, maxLength, currentValue)
{
    // 指定された最大桁数を超える入力を防ぐ
    // ただし、バックスペース、DELETE、タブキー 通常の数字キー、テンキーの数字は許可する
    if (e.type === 'keydown' &&
        currentValue.length >= maxLength &&
        e.keyCode !== 8 && e.keyCode !== 46 && e.keyCode !== 9 &&
        (e.keyCode < 48 || e.keyCode > 57) &&
        (e.keyCode < 96 || e.keyCode > 105))
    {
        return false;
    }

    return true;
}

// 入力制限判定ロジック(英数字のみ) 
function isAlphaNumeric(e) {
    var keyCd = e.keyCode; 
    if (e.type === 'keydown' &&
        (keyCd == 8 || // バックスペース許可
        keyCd == 9  || // Tab許可
        keyCd == 46 || // DELETE許可
        keyCd == 13 || // ENter許可
        keyCd == 37 || // ←許可
        keyCd == 39 || // →許可
        (keyCd >= 48 && keyCd <= 57)    || // 通常の数字許可
        (keyCd >= 65 && keyCd <= 90)    || // 英大文字許可
        (keyCd >= 97 && keyCd <= 122)   || // 英小文字許可
        (keyCd >= 96 && keyCd <= 105))) {  // テンキーの数字許可 
        return true;
    }
    else if (e.type === 'paste') // 貼り付けも許可
    { 
        return true;
    }
    return false;
}

/******************************************************************************************************
 *  ボタン
 ******************************************************************************************************/ 
$(document).ready(function () {
    // サブミットボタンがクリックされた時の処理
    $('form').on('submit', function () {
        // サブミットボタンを取得
        var submitButton = $(this).find('input[type="submit"], button[type="submit"]');

        // サブミットボタンを無効
        submitButton.prop('disabled', true);

        // 20秒後に戻る
        setTimeout(function () {
            submitButton.prop('disabled', false);
        }, 20000);
    });
});

/******************************************************************************************************
 *  getApiApprovalStatuses  管理番号でワークフローステータスを取得する
 ******************************************************************************************************/
function getApiApprovalStatuses(issueId, Lang) {
    //カテゴリのためのサイト詳細抽出
    let url = "https://" + location.hostname + "/WorkFlowWebApi/WorkFlowApprovalStatus/" + Lang + "/" + issueId.toString();
    return new Promise((resolve, reject) => {
        $.getJSON(url, (data) => {
            resolve(data);
        }).fail(function (jqXHR, textStatus, error) {
            reject(error);
        });
    });
}

/******************************************************************************************************
 *  getNantokaByAuth  プロセスID、グループID、ロールIDに紐づく権限情報を取得
 ******************************************************************************************************/
function getNantokaByAuth(beforeProcessId, beforeGroupId, beforeRoleId) {
    //カテゴリのためのサイト詳細抽出
    let url = "https://" + location.hostname +
        "/HinpoIdentityWebApi/HinpoIdentityGetAspNetRoles/" +
                                beforeProcessId.toString()  + "/" +
                                beforeGroupId.toString()    + "/" +
                                beforeRoleId.toString();
    return new Promise((resolve, reject) => {
        $.getJSON(url, (data) => {
            resolve(data);
        }).fail(function (jqXHR, textStatus, error) {
            reject(error);
        });
    });
}

/******************************************************************************************************
 *  getShikakariHatei  仕掛中かそうでないかを取得。また、仕掛中であれば、そのステータスの権限をみて
 *                     権限有りならば仕掛中と判定、そうでなければ仕掛中以外と判定
 ******************************************************************************************************/
async function getShikakariHatei(loginUserRole, issueId, Lang, Processid) {
    // ここでは、材料特採は考慮しない、閲覧専用であることが前提であるからこの処理は不要
    try {
        pApprovalStatuses = await getApiApprovalStatuses(issueId, Lang);
        if (pApprovalStatuses) {
            const count = pApprovalStatuses.length;
            let index = 0;
            isShikakari = true; // 初期値はtrueの仕掛中  false:仕掛中以外 true:仕掛中
            let beforeProcessId   = 0;
            let beforeGroupId     = 0;
            let beforeRoleId      = 0;

            // そもそもワークフローのステータスリストが0件の時は無条件に仕掛中 判定
            if (count === 0) {
                isShikakari = true;   // 仕掛中
                // 権限取得ように最初の工程のステータスの情報をセット
                beforeProcessId = Processid; // リンクやボタン押下時された列のWFのプロセスIDを渡す 
                beforeGroupId = 1;           // 起票部署
                beforeRoleId = 1;            // 申請や作成
            }
            else { 
                // WFステータスの降順でループ
                for (let item of pApprovalStatuses) {
                    if (count === index + 1) {              // 最後のループ
                        if (item.FinishFlg === true) {
                            isShikakari = true;             // 仕掛中
                            break;
                        }
                    } else {                                // 一番直近の処理済み工程の情報を取得
                        if (item.FinishFlg === true) {
                            if (index === 0) {
                                // のっけからFinishFlg == trueだったら最後の工程が完了済みなので仕掛中でない
                                isShikakari = false;        // 仕掛中以外
                                beforeProcessId = item.Processid;
                                beforeGroupId   = item.GroupId;
                                beforeRoleId    = item.RoleId;
                                break;
                            } else {
                                // 1回目のループ以降のFinishFlgがtrueは 仕掛かり判定
                                isShikakari     = true;     // 仕掛中 
                                break;
                            }
                        }
                        beforeProcessId = item.Processid;
                        beforeGroupId   = item.GroupId;
                        beforeRoleId    = item.RoleId;
                    }
                    index = index + 1;
                }
            } 

            // 今開こうとしている詳細画面が仕掛中と判定された場合のみの処理 
            // ログインユーザの所有する権限と現在仕掛かり中のステータスの権限をチェック
            // 権限がなければ  仕掛中以外として閲覧モード、
            // 権限があれば    仕掛中として    編集モードにする

            // 1:ProcessIdとGroupIdとRoleIdで現在仕掛かり中の捺印欄のロールを取得
            var activeStatusRole;

            // 仕掛中判定の場合のみの処理
            if (isShikakari === true) {

                activeStatusRole = await getNantokaByAuth(beforeProcessId, beforeGroupId, beforeRoleId);

                if (activeStatusRole.length > 0) {
                    // loginUserRoleの中に仕掛中
                    if (loginUserRole.includes(activeStatusRole[0].Name)) {
                        // 権限あり
                        isShikakari = true;      // 権限ありの場合、仕掛中 
                    } else {
                        // 権限無し
                        isShikakari = false;     // 権限無しの場合、仕掛中以外
                    }
                }
                else {
                    isShikakari = false;         // activeStatusRoleが0件の場合、仕掛中以外
                }
            }
        }
    } catch (error) {
        alert('error WorkFlowApprovalStatus:' + error);
    }
}