// simplified-main.js
(function () {
    'use strict';

    // ========================================
    // Navigation Functions
    // ========================================
    function initNavigation() {
        const navItems = document.querySelectorAll('.nav-item');

        navItems.forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                const page = item.getAttribute('data-page');

                navItems.forEach(nav => nav.classList.remove('active'));
                item.classList.add('active');

                switch (page) {
                    case 'home':
                        window.location.href = '/Home/Index';
                        break;
                    case 'mycourses':
                        window.location.href = '/MyCourses/Index';
                        break;
                    case 'points':
                        window.location.href = '/Balance/Index';
                        break;
                    case 'institutes':
                        window.location.href = '/Institutes/Index';
                        break;
                }
            });
        });

        setActiveNavItem();
    }

    function setActiveNavItem() {
        const currentPath = window.location.pathname.toLowerCase();
        const navItems = document.querySelectorAll('.nav-item');

        navItems.forEach(item => {
            item.classList.remove('active');
            const page = item.getAttribute('data-page');

            if (
                (page === 'home' && (currentPath === '/' || currentPath.includes('/home'))) ||
                (page === 'mycourses' && currentPath.includes('/mycourses')) ||
                (page === 'points' && currentPath.includes('/balance')) ||
                (page === 'institutes' && currentPath.includes('/institutes'))
            ) {
                item.classList.add('active');
            }
        });
    }

    // ========================================
    // Global Toast Function
    // ========================================
    function showToast(message, type = 'success') {
        // إزالة الرسائل السابقة
        const oldToasts = document.querySelectorAll('.success-toast');
        oldToasts.forEach(toast => toast.remove());

        // إنشاء عنصر التوست
        const toast = document.createElement('div');
        toast.className = 'success-toast';
        toast.style.background = type === 'error' ? '#ef4444' : type === 'warning' ? '#f59e0b' : '#10b981';
        toast.innerHTML = `
            <span>${type === 'error' ? '⚠️' : type === 'warning' ? '⚠️' : '✅'}</span>
            <span>${message}</span>
        `;

        // إضافة التأثيرات
        toast.style.cssText = `
            position: fixed;
            top: 20px;
            left: 50%;
            transform: translateX(-50%);
            color: white;
            padding: 16px 24px;
            border-radius: 8px;
            font-size: 15px;
            font-weight: 600;
            z-index: 1000;
            box-shadow: 0 8px 32px rgba(0,0,0,0.16);
            display: flex;
            align-items: center;
            gap: 10px;
            max-width: 90%;
            text-align: center;
            transition: opacity 0.3s ease;
        `;

        document.body.appendChild(toast);

        // إزالة التوست بعد 3 ثواني
        setTimeout(() => {
            toast.style.opacity = '0';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    // ========================================
    // Initialize
    // ========================================
    document.addEventListener('DOMContentLoaded', () => {
        initNavigation();

        // جعل showToast متاحة عالمياً
        window.showToast = showToast;

        // تهيئة صفحة النقاط إذا كانت موجودة
        if (document.querySelector('.mobile-points-container')) {
            initializePointsPage();
        }
    });

    // ========================================
    // Points Page Functions
    // ========================================
    function initializePointsPage() {
        let searchTimeout;
        let selectedUser = null;
        let isSearching = false;
        let isRedeeming = false;

        // عناصر DOM
        const toUserName = document.getElementById('toUserName');
        const clearSearchBtn = document.getElementById('clearSearchBtn');
        const userSearchResults = document.getElementById('userSearchResults');
        const selectedUserBadge = document.getElementById('selectedUserBadge');
        const removeUserBtn = document.getElementById('removeUserBtn');
        const toUserId = document.getElementById('toUserId');
        const transferForm = document.getElementById('transferForm');
        const redeemCodeForm = document.getElementById('redeemCodeForm');
        const redeemCodeInput = document.getElementById('redeemCodeInput');

        // التحقق من وجود العناصر الأساسية
        if (!toUserName || !transferForm) return;

        // ========================================
        // User Search Functions
        // ========================================
        toUserName.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            const query = this.value.trim();

            // إظهار/إخفاء زر المسح
            if (clearSearchBtn) {
                if (query.length > 0) {
                    clearSearchBtn.classList.add('show');
                } else {
                    clearSearchBtn.classList.remove('show');
                }
            }

            // إخفاء المستخدم المحدد عند الكتابة
            if (selectedUser && query !== selectedUser.name) {
                selectedUser = null;
                if (toUserId) toUserId.value = '';
                if (selectedUserBadge) selectedUserBadge.style.display = 'none';
            }

            if (query.length < 2) {
                if (userSearchResults) userSearchResults.style.display = 'none';
                return;
            }

            searchTimeout = setTimeout(() => {
                searchUsers(query);
            }, 500);
        });

        if (clearSearchBtn) {
            clearSearchBtn.addEventListener('click', function () {
                toUserName.value = '';
                if (toUserId) toUserId.value = '';
                if (userSearchResults) userSearchResults.style.display = 'none';
                if (selectedUserBadge) selectedUserBadge.style.display = 'none';
                selectedUser = null;
                this.classList.remove('show');
                toUserName.focus();
            });
        }

        if (removeUserBtn) {
            removeUserBtn.addEventListener('click', function () {
                toUserName.value = '';
                if (toUserId) toUserId.value = '';
                if (userSearchResults) userSearchResults.style.display = 'none';
                if (selectedUserBadge) selectedUserBadge.style.display = 'none';
                selectedUser = null;
                if (clearSearchBtn) clearSearchBtn.classList.remove('show');
                toUserName.focus();
            });
        }

        // دعم التنقل بلوحة المفاتيح في البحث
        toUserName.addEventListener('keydown', function (e) {
            if (userSearchResults && userSearchResults.style.display !== 'none') {
                const items = userSearchResults.querySelectorAll('.user-result-item');
                let currentIndex = -1;

                items.forEach((item, index) => {
                    if (item.classList.contains('selected')) {
                        currentIndex = index;
                    }
                });

                if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    currentIndex = Math.min(currentIndex + 1, items.length - 1);
                    updateSelection(items, currentIndex);
                } else if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    currentIndex = Math.max(currentIndex - 1, 0);
                    updateSelection(items, currentIndex);
                } else if (e.key === 'Enter') {
                    e.preventDefault();
                    if (currentIndex >= 0 && currentIndex < items.length) {
                        items[currentIndex].click();
                    }
                } else if (e.key === 'Escape') {
                    if (userSearchResults) userSearchResults.style.display = 'none';
                }
            }
        });

        function updateSelection(items, currentIndex) {
            items.forEach((item, index) => {
                if (index === currentIndex) {
                    item.classList.add('selected');
                    item.scrollIntoView({ block: 'nearest' });
                } else {
                    item.classList.remove('selected');
                }
            });
        }

        function searchUsers(query) {
            if (isSearching) return;
            isSearching = true;

            if (userSearchResults) {
                userSearchResults.innerHTML = `
                    <div class="search-loading">
                        <div class="search-loading-spinner"></div>
                        <span>جاري البحث...</span>
                    </div>
                `;
                userSearchResults.style.display = 'block';
            }

            fetch(`/Balance/SearchUsers?query=${encodeURIComponent(query)}`)
                .then(response => response.json())
                .then(data => {
                    if (data.success && data.users && data.users.length > 0) {
                        displayUserResults(data.users);
                    } else {
                        if (userSearchResults) {
                            userSearchResults.innerHTML = `
                                <div class="no-results">
                                    <div class="no-results-icon">🔍</div>
                                    <div>لا يوجد مستخدمين مطابقين</div>
                                    <small style="color: #94a3b8;">حاول بكلمات مختلفة</small>
                                </div>
                            `;
                            userSearchResults.style.display = 'block';
                        }
                    }
                })
                .catch(error => {
                    console.error('Error searching users:', error);
                    showToast('حدث خطأ في البحث عن المستخدمين', 'error');
                    if (userSearchResults) {
                        userSearchResults.innerHTML = `
                            <div class="no-results">
                                <div class="no-results-icon">⚠️</div>
                                <div>حدث خطأ في البحث</div>
                                <small style="color: #94a3b8;">يرجى المحاولة مرة أخرى</small>
                            </div>
                        `;
                        userSearchResults.style.display = 'block';
                    }
                })
                .finally(() => {
                    isSearching = false;
                });
        }

        function displayUserResults(users) {
            if (!userSearchResults) return;

            let html = '';
            users.forEach(user => {
                const initial = (user.fullName || user.userName).charAt(0).toUpperCase();
                const avatarHtml = user.avatarUrl
                    ? `<div class="user-avatar has-image"><img src="${user.avatarUrl}" alt="${user.userName}" onerror="this.parentElement.classList.remove('has-image'); this.parentElement.textContent='${initial}'"><span class="user-online-status"></span></div>`
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
                            <div class="user-name">${user.userName}</div>
                            ${fullNameHtml}
                            <div class="user-email">${user.email}</div>
                        </div>
                        <span class="user-check-icon">✓</span>
                    </div>
                `;
            });

            userSearchResults.innerHTML = html;
            userSearchResults.style.display = 'block';

            const resultItems = userSearchResults.querySelectorAll('.user-result-item');
            resultItems.forEach(item => {
                item.addEventListener('click', function () {
                    const userId = this.dataset.id;
                    const userName = this.dataset.name;
                    const email = this.dataset.email;
                    const fullName = this.dataset.fullname;
                    const avatarUrl = this.dataset.avatar;

                    selectUser(userId, userName, email, fullName, avatarUrl, this);
                });
            });
        }

        function selectUser(userId, userName, email, fullName, avatarUrl, element) {
            selectedUser = {
                id: userId,
                name: userName,
                email: email,
                fullName: fullName,
                avatarUrl: avatarUrl
            };

            if (toUserId) toUserId.value = userId;
            toUserName.value = userName;
            if (userSearchResults) userSearchResults.style.display = 'none';
            if (clearSearchBtn) clearSearchBtn.classList.add('show');

            if (selectedUserBadge) {
                const selectedUserAvatar = document.getElementById('selectedUserAvatar');
                const selectedUserName = document.getElementById('selectedUserName');
                const selectedUserEmail = document.getElementById('selectedUserEmail');

                const initial = (fullName || userName).charAt(0).toUpperCase();

                if (selectedUserAvatar) {
                    if (avatarUrl) {
                        selectedUserAvatar.innerHTML = `<img src="${avatarUrl}" alt="${fullName || userName}" style="width:100%;height:100%;object-fit:cover;border-radius:50%;">`;
                    } else {
                        selectedUserAvatar.textContent = initial;
                    }
                }

                if (selectedUserName) selectedUserName.textContent = fullName || userName;
                if (selectedUserEmail) selectedUserEmail.textContent = email;
                selectedUserBadge.style.display = 'flex';
            }

            showToast(`تم اختيار: ${fullName || userName}`, 'success');
        }

        // إغلاق نتائج البحث عند النقر خارج الصندوق
        document.addEventListener('click', function (e) {
            const searchWrapper = document.querySelector('.user-search-wrapper');
            if (searchWrapper && !searchWrapper.contains(e.target)) {
                if (userSearchResults) userSearchResults.style.display = 'none';
            }
        });

        // ========================================
        // Transfer Form Handler
        // ========================================
        transferForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const amountInput = document.getElementById('amount');
            const descriptionInput = document.getElementById('description');
            const transferBtn = document.getElementById('transferBtn');
            const transferBtnText = document.getElementById('transferBtnText');

            const toUserIdValue = toUserId ? toUserId.value : '';
            const toUserNameValue = selectedUser ? (selectedUser.fullName || selectedUser.name) : toUserName.value;
            const amount = parseFloat(amountInput ? amountInput.value : 0);
            const description = descriptionInput ? descriptionInput.value.trim() : '';

            // التحقق من صحة البيانات
            if (!toUserIdValue) {
                showToast('يرجى اختيار مستخدم مستلم', 'error');
                toUserName.focus();
                return;
            }

            if (!amount || amount <= 0) {
                showToast('يرجى إدخال مبلغ صحيح', 'error');
                if (amountInput) amountInput.focus();
                return;
            }

            const maxAmount = parseFloat(amountInput?.max || '0');
            if (amount > maxAmount) {
                showToast(`الرصيد غير كافي. الرصيد المتاح: ${maxAmount} نقطة`, 'error');
                if (amountInput) amountInput.focus();
                return;
            }

            // تأكيد التحويل
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'تأكيد التحويل',
                    html: `
                        <div style="text-align: right;">
                            <p style="margin-bottom: 10px;">هل أنت متأكد من تحويل النقاط؟</p>
                            <div style="background: #f8f9fa; padding: 15px; border-radius: 8px; margin: 15px 0;">
                                <p style="margin: 5px 0;"><strong>المستلم:</strong> ${toUserNameValue}</p>
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
                        performTransfer(toUserIdValue, amount, description);
                    }
                });
            } else {
                if (confirm(`هل أنت متأكد من تحويل ${amount} نقطة إلى ${toUserNameValue}؟`)) {
                    performTransfer(toUserIdValue, amount, description);
                }
            }
        });

        function performTransfer(toUserIdValue, amount, description) {
            const transferBtn = document.getElementById('transferBtn');
            const transferBtnText = document.getElementById('transferBtnText');

            if (transferBtn) transferBtn.disabled = true;
            if (transferBtnText) transferBtnText.innerHTML = '<span class="loading-spinner"></span> جاري التحويل...';

            const formData = new FormData();
            formData.append('toUserId', toUserIdValue);
            formData.append('amount', amount);
            formData.append('description', description);

            fetch('/Balance/TransferPoints', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        if (typeof Swal !== 'undefined') {
                            Swal.fire({
                                title: 'تم التحويل بنجاح! 🎉',
                                text: data.message,
                                icon: 'success',
                                confirmButtonText: 'حسناً'
                            }).then(() => {
                                location.reload();
                            });
                        } else {
                            showToast(data.message, 'success');
                            setTimeout(() => location.reload(), 1500);
                        }
                    } else {
                        showToast(data.message, 'error');
                    }
                })
                .catch(error => {
                    console.error('Error transferring points:', error);
                    showToast('حدث خطأ في الاتصال بالخادم', 'error');
                })
                .finally(() => {
                    if (transferBtn) transferBtn.disabled = false;
                    if (transferBtnText) transferBtnText.textContent = 'تحويل النقاط';
                });
        }

        // ========================================
        // Redeem Code Form Handler
        // ========================================
        if (redeemCodeForm && redeemCodeInput) {
            // تنسيق الكود أثناء الكتابة
            redeemCodeInput.addEventListener('input', function () {
                let value = this.value.toUpperCase().replace(/[^A-Z0-9]/g, '');

                // إضافة الشرطات التلقائية
                if (value.length > 4) {
                    value = value.substring(0, 4) + '-' + value.substring(4);
                }
                if (value.length > 9) {
                    value = value.substring(0, 9) + '-' + value.substring(9);
                }

                this.value = value;
            });

            // معالجة إرسال النموذج
            redeemCodeForm.addEventListener('submit', function (e) {
                e.preventDefault();

                const code = redeemCodeInput.value.trim();

                // التحقق من صحة الكود
                if (!code) {
                    showToast('يرجى إدخال الكود', 'error');
                    redeemCodeInput.focus();
                    return;
                }

                // التحقق من تنسيق الكود
                const codePattern = /^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$/;
                if (!codePattern.test(code)) {
                    showToast('صيغة الكود غير صحيحة. يجب أن تكون XXXX-XXXX-XXXX', 'error');
                    redeemCodeInput.focus();
                    return;
                }

                // منع الإرسال المتكرر
                if (isRedeeming) return;
                isRedeeming = true;

                // تعطيل زر الاسترداد أثناء المعالجة
                const submitBtn = redeemCodeForm.querySelector('button[type="submit"]');
                const originalBtnText = submitBtn ? submitBtn.textContent : '';

                if (submitBtn) {
                    submitBtn.disabled = true;
                    submitBtn.innerHTML = '<span class="loading-spinner"></span> جاري الاسترداد...';
                }

                // إرسال طلب الاسترداد
                const formData = new FormData();
                formData.append('code', code);

                fetch('/Balance/RedeemCode', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                })
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Network response was not ok');
                        }
                        return response.json();
                    })
                    .then(data => {
                        if (data.success) {
                            // نجاح الاسترداد
                            if (typeof Swal !== 'undefined') {
                                Swal.fire({
                                    title: 'تم الاسترداد بنجاح! 🎉',
                                    text: data.message || 'تم إضافة النقاط إلى رصيدك بنجاح',
                                    icon: 'success',
                                    confirmButtonText: 'حسناً'
                                }).then(() => {
                                    location.reload();
                                });
                            } else {
                                showToast(data.message || 'تم استرداد الكود بنجاح', 'success');
                                setTimeout(() => location.reload(), 2000);
                            }
                        } else {
                            // فشل الاسترداد
                            showToast(data.message || 'فشل استرداد الكود', 'error');

                            // إعادة تفعيل الحقل للسماح بالمحاولة مرة أخرى
                            redeemCodeInput.focus();
                        }
                    })
                    .catch(error => {
                        console.error('Error redeeming code:', error);
                        showToast('حدث خطأ في استرداد الكود. يرجى المحاولة مرة أخرى', 'error');
                    })
                    .finally(() => {
                        // إعادة تفعيل الزر
                        if (submitBtn) {
                            submitBtn.disabled = false;
                            submitBtn.textContent = originalBtnText;
                        }
                        isRedeeming = false;
                    });
            });
        }

        // ========================================
        // Amount Input Validation
        // ========================================
        const amountInput = document.getElementById('amount');
        if (amountInput) {
            amountInput.addEventListener('input', function () {
                if (parseFloat(this.value) < 0) {
                    this.value = '';
                }
            });
        }

        // ========================================
        // Scroll Functions
        // ========================================
        window.scrollToTransfer = function () {
            document.getElementById('transferSection')?.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        };

        window.scrollToTransactions = function () {
            document.getElementById('transactionsSection')?.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        };
    }
})();

$(document).ready(function () {
    initUserMenu();
    initModals();
});

function initUserMenu() {
    const $userMenuBtn = $('#userMenuBtn');
    const $userDropdown = $('#userDropdown');

    if (!$userMenuBtn.length || !$userDropdown.length) {
        return;
    }

    $userMenuBtn.on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $userDropdown.toggleClass('show');
        $userMenuBtn.toggleClass('active');
    });

    $(document).on('click', function (e) {
        if (!$userMenuBtn.is(e.target) &&
            $userMenuBtn.has(e.target).length === 0 &&
            !$userDropdown.is(e.target) &&
            $userDropdown.has(e.target).length === 0) {
            $userDropdown.removeClass('show');
            $userMenuBtn.removeClass('active');
        }
    });

    $(document).on('keydown', function (e) {
        if (e.key === 'Escape') {
            $userDropdown.removeClass('show');
            $userMenuBtn.removeClass('active');
        }
    });
}

function initModals() {
    const $settingsBtn = $('#settingsBtn');
    const $loginBtn = $('#loginBtn');
    const $settingsModal = $('#settingsModal');
    const $loginModal = $('#loginModal');

    if ($settingsBtn.length && $settingsModal.length) {
        $settingsBtn.on('click', function () {
            $settingsModal.css('display', 'flex');
        });
    }

    if ($loginBtn.length && $loginModal.length) {
        $loginBtn.on('click', function () {
            $loginModal.css('display', 'flex');
        });
    }

    $('.close-modal').on('click', function () {
        $(this).closest('.modal').css('display', 'none');
    });

    $(window).on('click', function (e) {
        if ($settingsModal.length && e.target === $settingsModal[0]) {
            $settingsModal.css('display', 'none');
        }
        if ($loginModal.length && e.target === $loginModal[0]) {
            $loginModal.css('display', 'none');
        }
    });
}