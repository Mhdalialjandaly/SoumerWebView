// ========================================
// Application State
// ========================================
let currentPage = 'home';

// ========================================
// DOM Elements
// ========================================
const mainContainer = document.getElementById('mainContainer');
const settingsBtn = document.getElementById('settingsBtn');
const loginBtn = document.getElementById('loginBtn');
const settingsModal = document.getElementById('settingsModal');
const loginModal = document.getElementById('loginModal');

// ========================================
// Teacher Card Handlers
// ========================================
function attachTeacherCardEvents() {
    document.querySelectorAll('.teacher-card').forEach(card => {
        card.removeEventListener('click', handleTeacherClick);
        card.addEventListener('click', handleTeacherClick);
    });
}

async function handleTeacherClick(e) {
    e.stopPropagation();
    const teacherId = this.getAttribute('data-teacher-id');

    try {
        const response = await fetch(`/Home/GetTeacherCourses?id=${teacherId}`);
        if (!response.ok) throw new Error('Network response was not ok');
        const teacher = await response.json();
        showTeacherCourses(teacher);
    } catch (error) {
        console.error('Error fetching teacher courses:', error);
        alert('حدث خطأ في تحميل بيانات المعلم');
    }
}

function showTeacherCourses(teacher) {
    const coursesHTML = `
        <div class="courses-page">
            <button class="back-home-btn" id="backToHomeBtn">
                ← العودة للرئيسية
            </button>
            <div style="background: white; border-radius: 1.5rem; padding: 1.2rem; text-align: center; margin-bottom: 1.2rem;">
                <img src="${teacher.image}" alt="${teacher.name}" class="teacher-profile-img">
                <h1 style="font-size: 1.4rem; margin-top: 0.8rem;">${teacher.name}</h1>
                <p style="color: #2563eb; font-weight: 600; font-size: 0.9rem;">${teacher.subject}</p>
                <p style="color: #334155; margin-top: 0.3rem; font-size: 0.8rem;">${teacher.bio}</p>
            </div>
            <h2 class="section-title" style="border-right-color:#f97316;">📘 كورسات ${teacher.name}</h2>
            <div style="display: flex; flex-wrap: wrap; gap: 0.8rem; justify-content: center; padding: 0.5rem 0;">
                ${teacher.courses.map(course => `
                    <div class="course-card">
                        <div style="font-size: 1.8rem;">📖</div>
                        <h3>${course}</h3>
                        <p>${teacher.subject}</p>
                        <button class="enroll-btn" onclick="alert('✨ تم إضافة الكورس إلى قائمة التسجيل الخاصة بك!')">تسجيل الآن</button>
                    </div>
                `).join('')}
            </div>
        </div>
    `;

    if (mainContainer) {
        mainContainer.innerHTML = coursesHTML;

        const backBtn = document.getElementById('backToHomeBtn');
        if (backBtn) {
            backBtn.addEventListener('click', () => {
                window.location.reload();
            });
        }

        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
}

// ========================================
// Navigation Functions
// ========================================
function updateActiveNav(pageId) {
    currentPage = pageId;
    document.querySelectorAll('.nav-item').forEach(item => {
        if (item.getAttribute('data-page') === pageId) {
            item.classList.add('active');
        } else {
            item.classList.remove('active');
        }
    });
}

function showHomePage() {
    window.location.reload();
}

function showMyCoursesPage() {
    if (!mainContainer) return;
    mainContainer.innerHTML = `
        <div class="temp-page">
            <div style="font-size: 4rem;">📚</div>
            <h2>كورساتي</h2>
            <p>هنا ستظهر جميع الكورسات التي قمت بالتسجيل فيها</p>
            <div style="display: flex; flex-wrap: wrap; gap: 1rem; justify-content: center; margin-top: 2rem;">
                <div class="course-card">
                    <div style="font-size: 2rem;">📖</div>
                    <h3>الرياضيات المتقدمة</h3>
                    <p>أ. أحمد المنصور</p>
                    <button class="enroll-btn" style="background: #10b981;">مستمر</button>
                </div>
                <div class="course-card">
                    <div style="font-size: 2rem;">📖</div>
                    <h3>فيزياء الكم</h3>
                    <p>د. ليلى حسن</p>
                    <button class="enroll-btn" style="background: #10b981;">مستمر</button>
                </div>
                <div class="course-card">
                    <div style="font-size: 2rem;">📖</div>
                    <h3>برمجة بايثون</h3>
                    <p>أ. عمر الجابري</p>
                    <button class="enroll-btn" style="background: #f59e0b;">قيد التقدم</button>
                </div>
            </div>
            <button class="back-home-btn" onclick="location.reload()" style="margin-top: 2rem;">← العودة للرئيسية</button>
        </div>
    `;
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function showPointsPage() {
    if (!mainContainer) return;
    mainContainer.innerHTML = `
        <div class="temp-page">
            <div style="font-size: 4rem;">💎</div>
            <h2>نقاطي</h2>
            <div style="background: linear-gradient(135deg, #2563eb, #1e40af); color: white; padding: 2rem; border-radius: 1.5rem; margin: 1rem auto; max-width: 300px;">
                <div style="font-size: 3rem; font-weight: bold;">1,250</div>
                <div style="font-size: 1rem;">نقطة</div>
            </div>
            <p>يمكنك استبدال النقاط بجوائز قيمة</p>
            <div style="margin-top: 2rem;">
                <button class="enroll-btn" style="background: #f59e0b;" onclick="alert('سيتم تفعيل خاصية استبدال النقاط قريباً')">استبدال النقاط</button>
            </div>
            <button class="back-home-btn" onclick="location.reload()" style="margin-top: 2rem;">← العودة للرئيسية</button>
        </div>
    `;
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function showInstitutesPage() {
    if (!mainContainer) return;
    mainContainer.innerHTML = `
        <div class="temp-page">
            <div style="font-size: 4rem;">🏫</div>
            <h2>المعاهد</h2>
            <p>اكتشف المعاهد التعليمية المتاحة</p>
            <div style="display: flex; flex-wrap: wrap; gap: 1rem; justify-content: center; margin-top: 2rem;">
                <div class="course-card">
                    <div style="font-size: 2rem;">🏛️</div>
                    <h3>معهد العلوم</h3>
                    <p>دورات في الرياضيات والفيزياء</p>
                    <button class="enroll-btn" onclick="alert('معهد العلوم - سيتم تفعيل قريباً')">عرض التفاصيل</button>
                </div>
                <div class="course-card">
                    <div style="font-size: 2rem;">📖</div>
                    <h3>أكاديمية اللغات</h3>
                    <p>تعلم اللغات مع خبراء</p>
                    <button class="enroll-btn" onclick="alert('أكاديمية اللغات - سيتم تفعيل قريباً')">عرض التفاصيل</button>
                </div>
                <div class="course-card">
                    <div style="font-size: 2rem;">💻</div>
                    <h3>معهد البرمجة</h3>
                    <p>تطوير مهارات البرمجة</p>
                    <button class="enroll-btn" onclick="alert('معهد البرمجة - سيتم تفعيل قريباً')">عرض التفاصيل</button>
                </div>
                <div class="course-card">
                    <div style="font-size: 2rem;">🎨</div>
                    <h3>معهد الفنون</h3>
                    <p>دورات في الرسم والتصميم</p>
                    <button class="enroll-btn" onclick="alert('معهد الفنون - سيتم تفعيل قريباً')">عرض التفاصيل</button>
                </div>
            </div>
            <button class="back-home-btn" onclick="location.reload()" style="margin-top: 2rem;">← العودة للرئيسية</button>
        </div>
    `;
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ========================================
// Modal Functions
// ========================================
function initModals() {
    if (settingsBtn) {
        settingsBtn.onclick = () => {
            if (settingsModal) settingsModal.style.display = 'flex';
        };
    }

    if (loginBtn) {
        loginBtn.onclick = () => {
            if (loginModal) loginModal.style.display = 'flex';
        };
    }

    const closeSettings = document.getElementById('closeSettingsModal');
    const closeLogin = document.getElementById('closeLoginModal');

    if (closeSettings) {
        closeSettings.onclick = () => {
            if (settingsModal) settingsModal.style.display = 'none';
        };
    }

    if (closeLogin) {
        closeLogin.onclick = () => {
            if (loginModal) loginModal.style.display = 'none';
        };
    }

    window.onclick = (e) => {
        if (settingsModal && e.target === settingsModal) settingsModal.style.display = 'none';
        if (loginModal && e.target === loginModal) loginModal.style.display = 'none';
    };
}

// ========================================
// Navigation Initialization
// ========================================
function initNavigation() {
    const navItems = document.querySelectorAll('.nav-item');
    navItems.forEach(item => {
        item.addEventListener('click', () => {
            const page = item.getAttribute('data-page');
            updateActiveNav(page);

            switch (page) {
                case 'home':
                    showHomePage();
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

// ========================================
// Initialize Application
// ========================================
document.addEventListener('DOMContentLoaded', () => {
    attachTeacherCardEvents();
    initModals();
    initNavigation();
});

// ========================================
// Re-attach events after dynamic content loads
// ========================================
const observer = new MutationObserver(() => {
    attachTeacherCardEvents();
});

if (mainContainer) {
    observer.observe(mainContainer, { childList: true, subtree: true });
}


// ========================================
// User Menu Dropdown
// ========================================
function initUserMenu() {
    const userMenuBtn = document.getElementById('userMenuBtn');
    const userDropdown = document.getElementById('userDropdown');

    if (userMenuBtn && userDropdown) {
        // Toggle dropdown on click
        userMenuBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            userDropdown.classList.toggle('show');
            userMenuBtn.classList.toggle('active');
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!userMenuBtn.contains(e.target) && !userDropdown.contains(e.target)) {
                userDropdown.classList.remove('show');
                userMenuBtn.classList.remove('active');
            }
        });

        // Close dropdown on ESC key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && userDropdown.classList.contains('show')) {
                userDropdown.classList.remove('show');
                userMenuBtn.classList.remove('active');
            }
        });
    }
}

// ========================================
// Initialize all components
// ========================================
document.addEventListener('DOMContentLoaded', () => {
    attachTeacherCardEvents();
    initModals();
    initNavigation();
    initUserMenu(); // إضافة تهيئة قائمة المستخدم
});