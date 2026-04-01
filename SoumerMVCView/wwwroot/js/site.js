// simplified-main.js - فقط للتفاعلات وليس للعرض
(function () {
    'use strict';

    // ========================================
    // Navigation Functions
    // ========================================
    function initNavigation() {
        const navItems = document.querySelectorAll('.nav-item');

        navItems.forEach(item => {
            item.addEventListener('click', (e) => {
                const page = item.getAttribute('data-page');

                // تحديث الحالة النشطة
                navItems.forEach(nav => nav.classList.remove('active'));
                item.classList.add('active');

                // التنقل بين الصفحات
                switch (page) {
                    case 'home':
                        // العودة للصفحة الرئيسية عن طريق إعادة التحميل
                        window.location.href = '/Home/Index';
                        break;
                    case 'mycourses':
                        showMyCoursesPage();
                        break;
                    case 'points':
                        showPointsPage();
                        break;
                    case 'institutes':
                        showInstitutesPage();
                        break;
                }
            });
        });
    }

    function showMyCoursesPage() {
        // استخدام fetch لجلب محتوى الصفحة من الخادم بدلاً من HTML ثابت
        fetch('/Home/MyCourses')
            .then(response => response.text())
            .then(html => {
                const mainContainer = document.getElementById('mainContainer');
                if (mainContainer) {
                    mainContainer.innerHTML = html;
                    window.scrollTo({ top: 0, behavior: 'smooth' });
                }
            })
            .catch(error => {
                console.error('Error loading my courses page:', error);
                // استخدام HTML احتياطي
                showFallbackMyCoursesPage();
            });
    }

    function showFallbackMyCoursesPage() {
        const mainContainer = document.getElementById('mainContainer');
        if (!mainContainer) return;

        mainContainer.innerHTML = `
            <div class="temp-page" style="padding: 2rem; text-align: center;">
                <div style="font-size: 4rem;">📚</div>
                <h2>كورساتي</h2>
                <p>هنا ستظهر جميع الكورسات التي قمت بالتسجيل فيها</p>
                <div style="display: flex; flex-wrap: wrap; gap: 1rem; justify-content: center; margin-top: 2rem;">
                    <div class="course-card" style="background: white; border-radius: 1rem; padding: 1rem; width: 250px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <div style="font-size: 2rem;">📖</div>
                        <h3>الرياضيات المتقدمة</h3>
                        <p>أ. أحمد المنصور</p>
                        <button class="enroll-btn" style="background: #10b981; color: white; border: none; padding: 0.5rem 1rem; border-radius: 0.5rem; cursor: pointer;">مستمر</button>
                    </div>
                    <div class="course-card" style="background: white; border-radius: 1rem; padding: 1rem; width: 250px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <div style="font-size: 2rem;">📖</div>
                        <h3>فيزياء الكم</h3>
                        <p>د. ليلى حسن</p>
                        <button class="enroll-btn" style="background: #10b981; color: white; border: none; padding: 0.5rem 1rem; border-radius: 0.5rem; cursor: pointer;">مستمر</button>
                    </div>
                </div>
                <button class="back-home-btn" onclick="window.location.href='/Home/Index'" style="margin-top: 2rem; background: #6c757d; color: white; border: none; padding: 0.5rem 1rem; border-radius: 0.5rem; cursor: pointer;">← العودة للرئيسية</button>
            </div>
        `;
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    function showPointsPage() {
        fetch('/Home/Points')
            .then(response => response.text())
            .then(html => {
                const mainContainer = document.getElementById('mainContainer');
                if (mainContainer) {
                    mainContainer.innerHTML = html;
                    window.scrollTo({ top: 0, behavior: 'smooth' });
                }
            })
            .catch(error => {
                console.error('Error loading points page:', error);
                showFallbackPointsPage();
            });
    }

    function showFallbackPointsPage() {
        const mainContainer = document.getElementById('mainContainer');
        if (!mainContainer) return;

        mainContainer.innerHTML = `
            <div class="temp-page" style="padding: 2rem; text-align: center;">
                <div style="font-size: 4rem;">💎</div>
                <h2>نقاطي</h2>
                <div style="background: linear-gradient(135deg, #2563eb, #1e40af); color: white; padding: 2rem; border-radius: 1.5rem; margin: 1rem auto; max-width: 300px;">
                    <div style="font-size: 3rem; font-weight: bold;">1,250</div>
                    <div style="font-size: 1rem;">نقطة</div>
                </div>
                <p>يمكنك استبدال النقاط بجوائز قيمة</p>
                <div style="margin-top: 2rem;">
                    <button class="enroll-btn" style="background: #f59e0b; color: white; border: none; padding: 0.5rem 1rem; border-radius: 0.5rem; cursor: pointer;" onclick="alert('سيتم تفعيل خاصية استبدال النقاط قريباً')">استبدال النقاط</button>
                </div>
                <button class="back-home-btn" onclick="window.location.href='/Home/Index'" style="margin-top: 2rem; background: #6c757d; color: white; border: none; padding: 0.5rem 1rem; border-radius: 0.5rem; cursor: pointer;">← العودة للرئيسية</button>
            </div>
        `;
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    function showInstitutesPage() {
        fetch('/Home/Institutes')
            .then(response => response.text())
            .then(html => {
                const mainContainer = document.getElementById('mainContainer');
                if (mainContainer) {
                    mainContainer.innerHTML = html;
                    window.scrollTo({ top: 0, behavior: 'smooth' });
                }
            })
            .catch(error => {
                console.error('Error loading institutes page:', error);
                showFallbackInstitutesPage();
            });
    }

    function showFallbackInstitutesPage() {
        const mainContainer = document.getElementById('mainContainer');
        if (!mainContainer) return;

        mainContainer.innerHTML = `
            <div class="temp-page" style="padding: 2rem; text-align: center;">
                <div style="font-size: 4rem;">🏫</div>
                <h2>المعاهد</h2>
                <p>اكتشف المعاهد التعليمية المتاحة</p>
                <div style="display: flex; flex-wrap: wrap; gap: 1rem; justify-content: center; margin-top: 2rem;">
                    <div class="course-card" style="background: white; border-radius: 1rem; padding: 1rem; width: 250px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <div style="font-size: 2rem;">🏛️</div>
                        <h3>معهد العلوم</h3>
                        <p>دورات في الرياضيات والفيزياء</p>
                        <button class="enroll-btn" style="background: #2563eb; color: white; border: none; padding: 0.5rem 1rem; border-radius: 0.5rem; cursor: pointer;" onclick="alert('معهد العلوم - سيتم تفعيل قريباً')">عرض التفاصيل</button>
                    </div>
                    <div class="course-card" style="background: white; border-radius: 1rem; padding: 1rem; width: 250px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <div style="font-size: 2rem;">📖</div>
                        <h3>أكاديمية اللغات</h3>
                        <p>تعلم اللغات مع خبراء</p>
                        <button class="enroll-btn" style="background: #2563eb; color: white; border: none; padding: 0.5rem 1rem; border-radius: 0.5rem; cursor: pointer;" onclick="alert('أكاديمية اللغات - سيتم تفعيل قريباً')">عرض التفاصيل</button>
                    </div>
                </div>
                <button class="back-home-btn" onclick="window.location.href='/Home/Index'" style="margin-top: 2rem; background: #6c757d; color: white; border: none; padding: 0.5rem 1rem; border-radius: 0.5rem; cursor: pointer;">← العودة للرئيسية</button>
            </div>
        `;
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    // ========================================
    // Modal Functions
    // ========================================
    function initModals() {
        const settingsBtn = document.getElementById('settingsBtn');
        const loginBtn = document.getElementById('loginBtn');
        const settingsModal = document.getElementById('settingsModal');
        const loginModal = document.getElementById('loginModal');

        if (settingsBtn && settingsModal) {
            settingsBtn.onclick = () => settingsModal.style.display = 'flex';

            const closeSettings = document.getElementById('closeSettingsModal');
            if (closeSettings) {
                closeSettings.onclick = () => settingsModal.style.display = 'none';
            }
        }

        if (loginBtn && loginModal) {
            loginBtn.onclick = () => loginModal.style.display = 'flex';

            const closeLogin = document.getElementById('closeLoginModal');
            if (closeLogin) {
                closeLogin.onclick = () => loginModal.style.display = 'none';
            }
        }

        window.onclick = (e) => {
            if (settingsModal && e.target === settingsModal) settingsModal.style.display = 'none';
            if (loginModal && e.target === loginModal) loginModal.style.display = 'none';
        };
    }

    function initUserMenu() {
        const userMenuBtn = document.getElementById('userMenuBtn');
        const userDropdown = document.getElementById('userDropdown');

        if (userMenuBtn && userDropdown) {
            userMenuBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                userDropdown.classList.toggle('show');
                userMenuBtn.classList.toggle('active');
            });

            document.addEventListener('click', (e) => {
                if (!userMenuBtn.contains(e.target) && !userDropdown.contains(e.target)) {
                    userDropdown.classList.remove('show');
                    userMenuBtn.classList.remove('active');
                }
            });
        }
    }

    // ========================================
    // Initialize
    // ========================================
    document.addEventListener('DOMContentLoaded', () => {
        initNavigation();
        initModals();
        initUserMenu();
    });
})();



function displayVideosModal(courseName, videos) {
    const modal = document.getElementById('videosModal');
    const modalTitle = document.getElementById('modalCourseTitle');
    const videosContainer = document.getElementById('videosListContainer');

    modalTitle.textContent = `📺 ${courseName} - قائمة الفيديوهات (${videos.length} فيديو)`;

    // تجميع الفيديوهات حسب الترتيب
    const sortedVideos = [...videos].sort((a, b) => a.order - b.order);

    videosContainer.innerHTML = `
        <div class="videos-list-modal">
            ${sortedVideos.map(video => `
                <div class="video-item-modal" onclick="playVideo(${video.id})">
                    <div class="video-thumbnail-modal">
                        <img src="${video.thumbnailUrl || '/images/video-placeholder.jpg'}"
                             alt="${escapeHtml(video.title)}"
                             onerror="this.src='/images/video-placeholder.jpg'">
                        ${video.duration > 0 ? `
                            <div class="video-duration-badge">
                                ${Math.floor(video.duration / 60)}:${(video.duration % 60).toString().padStart(2, '0')}
                            </div>
                        ` : ''}
                        <div class="play-overlay">▶️</div>
                    </div>
                    <div class="video-info-modal">
                        <div class="video-order-badge">#${video.order}</div>
                        <h4>${escapeHtml(video.title)}</h4>
                        <p>${escapeHtml(video.description ? (video.description.length > 100 ? video.description.substring(0, 100) + '...' : video.description) : 'لا يوجد وصف')}</p>
                        <div class="video-meta">
                            <span class="platform-badge">
                                ${video.platform === 1 ? '📺 يوتيوب' : '🎬 فيميو'}
                            </span>
                            ${video.isFree ?
            '<span class="free-badge">🎁 مجاني</span>' :
            '<span class="premium-badge">💎 مدفوع</span>'
        }
                        </div>
                    </div>
                </div>
            `).join('')}
        </div>
    `;

    // إضافة تأثيرات CSS ديناميكية للمودال
    const style = document.createElement('style');
    style.textContent = `
        .video-item-modal {
            display: flex;
            gap: 1rem;
            padding: 1rem;
            background: #f8fafc;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.2s;
            margin-bottom: 0.75rem;
        }
        .video-item-modal:hover {
            background: #f1f5f9;
            transform: translateX(-4px);
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }
        .video-thumbnail-modal {
            position: relative;
            width: 160px;
            flex-shrink: 0;
            border-radius: 8px;
            overflow: hidden;
        }
        .video-thumbnail-modal img {
            width: 100%;
            height: 90px;
            object-fit: cover;
        }
        .video-duration-badge {
            position: absolute;
            bottom: 4px;
            right: 4px;
            background: rgba(0,0,0,0.7);
            color: white;
            padding: 0.1rem 0.3rem;
            border-radius: 4px;
            font-size: 0.6rem;
        }
        .play-overlay {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            font-size: 2rem;
            opacity: 0;
            transition: opacity 0.2s;
            text-shadow: 0 0 10px rgba(0,0,0,0.5);
        }
        .video-item-modal:hover .play-overlay {
            opacity: 1;
        }
        .video-info-modal {
            flex: 1;
        }
        .video-order-badge {
            display: inline-block;
            background: #e2e8f0;
            color: #475569;
            padding: 0.15rem 0.5rem;
            border-radius: 12px;
            font-size: 0.6rem;
            margin-bottom: 0.5rem;
        }
        .video-info-modal h4 {
            margin: 0 0 0.5rem 0;
            font-size: 0.9rem;
            color: #1e293b;
        }
        .video-info-modal p {
            margin: 0 0 0.5rem 0;
            font-size: 0.75rem;
            color: #64748b;
            line-height: 1.4;
        }
        .video-meta {
            display: flex;
            gap: 0.5rem;
            flex-wrap: wrap;
        }
        .platform-badge {
            font-size: 0.65rem;
            color: #94a3b8;
        }
        .free-badge {
            background: #d1fae5;
            color: #065f46;
            padding: 0.15rem 0.5rem;
            border-radius: 12px;
            font-size: 0.65rem;
        }
        .premium-badge {
            background: #fef3c7;
            color: #92400e;
            padding: 0.15rem 0.5rem;
            border-radius: 12px;
            font-size: 0.65rem;
        }
        @media (max-width: 640px) {
            .video-item-modal {
                flex-direction: column;
            }
            .video-thumbnail-modal {
                width: 100%;
            }
            .video-thumbnail-modal img {
                height: 120px;
            }
        }
    `;

    // إزالة الأنماط القديمة إذا وجدت
    const oldStyle = document.getElementById('videos-modal-styles');
    if (oldStyle) oldStyle.remove();

    style.id = 'videos-modal-styles';
    document.head.appendChild(style);

    modal.style.display = 'flex';

    // إضافة تأثير ظهور سلس
    modal.style.animation = 'fadeIn 0.2s ease';
}