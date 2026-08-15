$(document).ready(function () {
    let searchTimeout;
    let selectedUser = null;
    let isSearching = false;

    // البحث عن المستخدمين مع تأخير
    $('#toUserName').on('input', function () {
        clearTimeout(searchTimeout);
        const query = $(this).val().trim();

        // إظهار/إخفاء زر المسح
        if (query.length > 0) {
            $('#clearSearchBtn').addClass('show');
        } else {
            $('#clearSearchBtn').removeClass('show');
        }

        // إخفاء المستخدم المحدد عند الكتابة
        if (selectedUser) {
            selectedUser = null;
            $('#toUserId').val('');
            $('#selectedUserBadge').hide();
        }

        if (query.length < 2) {
            $('#userSearchResults').hide();
            return;
        }

        searchTimeout = setTimeout(() => {
            searchUsers(query);
        }, 500);
    });

    // زر مسح البحث
    $('#clearSearchBtn').click(function () {
        $('#toUserName').val('');
        $('#toUserId').val('');
        $('#userSearchResults').hide();
        $('#selectedUserBadge').hide();
        selectedUser = null;
        $(this).removeClass('show');
        $('#toUserName').focus();
    });

    // إزالة المستخدم المحدد
    $('#removeUserBtn').click(function () {
        $('#toUserName').val('');
        $('#toUserId').val('');
        $('#userSearchResults').hide();
        $('#selectedUserBadge').hide();
        selectedUser = null;
        $('#clearSearchBtn').removeClass('show');
        $('#toUserName').focus();
    });

    // البحث عن المستخدمين
    function searchUsers(query) {
        if (isSearching) return;

        isSearching = true;

        // إظهار مؤشر التحميل
        $('#userSearchResults').html(
            '<div class="search-loading">' +
            '<div class="search-loading-spinner"></div>' +
            '<span>جاري البحث...</span>' +
            '</div>'
        ).show();

        $.ajax({
            url: '/Balance/SearchUsers',
            type: 'GET',
            data: { query: query },
            success: function (response) {
                if (response.success && response.users && response.users.length > 0) {
                    displayUserResults(response.users);
                } else {
                    $('#userSearchResults').html(
                        '<div class="no-results">' +
                        '<div class="no-results-icon">🔍</div>' +
                        '<div>لا يوجد مستخدمين مطابقين</div>' +
                        '<small style="color: #94a3b8;">حاول بكلمات مختلفة</small>' +
                        '</div>'
                    ).show();
                }
            },
            error: function () {
                $('#userSearchResults').html(
                    '<div class="no-results">' +
                    '<div class="no-results-icon">⚠️</div>' +
                    '<div>حدث خطأ في البحث</div>' +
                    '<small style="color: #94a3b8;">يرجى المحاولة مرة أخرى</small>' +
                    '</div>'
                ).show();
            },
            complete: function () {
                isSearching = false;
            }
        });
    }

    // عرض نتائج البحث
    function displayUserResults(users) {
        let html = '';
        users.forEach(user => {
            const initial = (user.fullName || user.userName).charAt(0).toUpperCase();
            const avatarHtml = user.avatarUrl
                ? `<div class="user-avatar has-image"><img src="${user.avatarUrl}" alt="${user.userName}"><span class="user-online-status"></span></div>`
                : `<div class="user-avatar">${initial}<span class="user-online-status"></span></div>`;

            const fullNameHtml = user.fullName
                ? `<div class="user-fullname">${user.fullName}</div>`
                : '';

            html += `
                        <div class="user-result-item" 
                             data-id="${user.id}" 
                             data-name="${user.userName}"
                             data-email="${user.email}"
                             data-fullname="${user.fullName || ''}"
                             data-avatar="${user.avatarUrl || ''}">
                            ${avatarHtml}
                            <div class="user-info">
                                <div class="user-name">
                                    ${user.userName}
                                    <span style="font-size: 11px; color: #94a3b8;">@${user.userName}</span>
                                </div>
                                ${fullNameHtml}
                                <div class="user-email">${user.email}</div>
                            </div>
                            <span class="user-check-icon">✓</span>
                        </div>
                    `;
        });

        $('#userSearchResults').html(html).show();

        // ربط حدث النقر على نتائج البحث
        $('.user-result-item').click(function () {
            const userId = $(this).data('id');
            const userName = $(this).data('name');
            const email = $(this).data('email');
            const fullName = $(this).data('fullname');
            const avatarUrl = $(this).data('avatar');

            selectUser(userId, userName, email, fullName, avatarUrl, $(this));
        });

        // دعم التنقل بلوحة المفاتيح
        let currentIndex = -1;
        const $items = $('.user-result-item');

        $('#toUserName').on('keydown', function (e) {
            if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
                e.preventDefault();

                if (e.key === 'ArrowDown') {
                    currentIndex = Math.min(currentIndex + 1, $items.length - 1);
                } else if (e.key === 'ArrowUp') {
                    currentIndex = Math.max(currentIndex - 1, 0);
                }

                $items.removeClass('selected');
                $items.eq(currentIndex).addClass('selected');
                $items.eq(currentIndex)[0].scrollIntoView({ block: 'nearest' });
            } else if (e.key === 'Enter') {
                e.preventDefault();
                if (currentIndex >= 0 && currentIndex < $items.length) {
                    $items.eq(currentIndex).click();
                }
            }
        });
    }

    // اختيار مستخدم
    function selectUser(userId, userName, email, fullName, avatarUrl, $element) {
        selectedUser = {
            id: userId,
            name: userName,
            email: email,
            fullName: fullName,
            avatarUrl: avatarUrl
        };

        $('#toUserId').val(userId);
        $('#toUserName').val(userName);
        $('#userSearchResults').hide();
        $('#clearSearchBtn').addClass('show');

        // تحديث شارة المستخدم المحدد
        const initial = (fullName || userName).charAt(0).toUpperCase();
        $('#selectedUserAvatar').text(initial);
        $('#selectedUserName').text(fullName || userName);
        $('#selectedUserEmail').text(email);
        $('#selectedUserBadge').show();

        // إظهار رسالة تأكيد
        showToast(`تم اختيار: ${fullName || userName}`, 'success');
    }

    // إغلاق نتائج البحث عند النقر خارج الصندوق
    $(document).click(function (e) {
        if (!$(e.target).closest('.user-search-wrapper').length) {
            $('#userSearchResults').hide();
        }
    });

    // تحويل النقاط
    $('#transferForm').submit(function (e) {
        e.preventDefault();

        const toUserId = $('#toUserId').val();
        const toUserName = selectedUser ? (selectedUser.fullName || selectedUser.name) : $('#toUserName').val();
        const amount = parseFloat($('#amount').val());
        const description = $('#description').val().trim();

        // التحقق من صحة البيانات
        if (!toUserId) {
            showToast('يرجى اختيار مستخدم مستلم', 'error');
            $('#toUserName').focus();
            return;
        }

        if (!amount || amount <= 0) {
            showToast('يرجى إدخال مبلغ صحيح', 'error');
            $('#amount').focus();
            return;
        }

        const maxAmount = @Model.CurrentBalance;
        if (amount > maxAmount) {
            showToast(`الرصيد غير كافي. الرصيد المتاح: ${maxAmount} نقطة`, 'error');
            $('#amount').focus();
            return;
        }

        // تأكيد التحويل
        Swal.fire({
            title: 'تأكيد التحويل',
            html: `
                        <div style="text-align: right;">
                            <p style="margin-bottom: 10px;">هل أنت متأكد من تحويل النقاط؟</p>
                            <div style="background: #f8f9fa; padding: 15px; border-radius: 8px; margin: 15px 0;">
                                <p style="margin: 5px 0;"><strong>المستلم:</strong> ${toUserName}</p>
                                <p style="margin: 5px 0;"><strong>المبلغ:</strong> ${amount} نقطة</p>
                                ${description ? `<p style="margin: 5px 0;"><strong>الوصف:</strong> ${description}</p>` : ''}
                            </div>
                        </div>
                    `,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'نعم، قم بالتحويل',
            cancelButtonText: 'إلغاء',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                performTransfer(toUserId, amount, description);
            }
        });
    });

    // تنفيذ التحويل
    function performTransfer(toUserId, amount, description) {
        const transferBtn = $('#transferBtn');
        const transferBtnText = $('#transferBtnText');
        transferBtn.prop('disabled', true);
        transferBtnText.html('<span class="loading-spinner"></span> جاري التحويل...');

        $.ajax({
            url: '/Balance/TransferPoints',
            type: 'POST',
            data: {
                toUserId: toUserId,
                amount: amount,
                description: description
            },
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        title: 'تم التحويل بنجاح! 🎉',
                        text: response.message,
                        icon: 'success',
                        confirmButtonText: 'حسناً'
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    showToast(response.message, 'error');
                }
            },
            error: function (xhr, status, error) {
                let errorMessage = 'حدث خطأ في الاتصال بالخادم';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                showToast(errorMessage, 'error');
            },
            complete: function () {
                transferBtn.prop('disabled', false);
                transferBtnText.text('تحويل النقاط');
            }
        });
    }

    // إظهار رسالة تنبيه
    function showToast(message, type = 'success') {
        const toast = $('<div>')
            .addClass('success-toast')
            .css('background', type === 'error' ? '#ef4444' : '#10b981')
            .text(message);

        $('body').append(toast);

        setTimeout(() => {
            toast.fadeOut(300, function () {
                $(this).remove();
            });
        }, 3000);
    }

    // التمرير إلى نموذج التحويل
    window.scrollToTransfer = function () {
        document.getElementById('transferSection').scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    };

    // التمرير إلى المعاملات
    window.scrollToTransactions = function () {
        document.getElementById('transactionsSection').scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    };
});