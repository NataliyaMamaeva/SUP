

document.addEventListener("DOMContentLoaded", () => {   
    loadEmployeesWithProjects();
    loadProjectsQuery();
});
function loadProjectsQuery() {
    fetch('/GetProjectsQuery')
        .then(response => response.json())
        .then(data => {
            console.log(data);

            const queryContainer = document.getElementById("newProjectsContainer");         
            data.forEach( project => {
                    const projectDiv = document.createElement("div");
                    projectDiv.className = "project";

                    const projectImage = document.createElement("img");
                    projectImage.src = project.sketchPath ? project.sketchPath : "images/default.jpg";
                    projectImage.alt = project.projectName;
                    projectDiv.appendChild(projectImage);

                    const projectText = document.createElement("div");
                    projectText.className = "project_text";

                    const projectName = document.createElement("b");
                    projectName.textContent = `${project.projectName}`;
                    projectText.appendChild(projectName);

                    const projectQuantity = document.createElement("p");
                    projectQuantity.textContent = `Тираж: ${project.quantity}`;
                    projectText.appendChild(projectQuantity);

                    const projectDate = document.createElement("p");
                    projectDate.textContent = `Дата: ${project.deadline}`;
                    projectText.appendChild(projectDate);

                    //const projectMaterial = document.createElement("p");
                    //projectMaterial.textContent = `Материал: ${project.material}`;
                    //projectText.appendChild(projectMaterial);

                projectDiv.appendChild(projectText);
                projectDiv.onclick = () => openProjectCard(project.projectId);
                queryContainer.appendChild(projectDiv);
                });           
            }).catch(error => {
            console.error("Ошибка при загрузке данных:", error);
        });
}
async function loadEmployeesWithProjects() {
    try {
        const response = await fetch('/GetEmployeesWithProjects');
        const data = await response.json();

        console.log(data);

        const employeesContainer = document.getElementById("employeesContainer");
        employeesContainer.innerHTML = "";

        const userResponse = await fetch('/api/GetCurrentUser');
        const currentUserInfo = await userResponse.json();
       // console.log(currentUserInfo);
        const isBoss = currentUserInfo.isBoss;
        const currentUserId = currentUserInfo.currentUserId;
       // console.log("isBoss: " + isBoss + "  currentUserId: " + currentUserId);

        data.forEach(async employee => {
            const employeeDiv = document.createElement("div");
            employeeDiv.className = "header";

            const employeeName = document.createElement("h3");
            employeeName.className = "master_name";
            employeeName.textContent = employee.employeeName;
            employeeDiv.appendChild(employeeName);

            if (isBoss || currentUserId === employee.employeeId) {
                const employeeCardButton = document.createElement("button");
                employeeCardButton.textContent = "Карточка сотрудника";
                employeeCardButton.onclick = () => loadEmployeeCard(employee.employeeId);
                employeeDiv.appendChild(employeeCardButton);
            }
        
            const planButton = document.createElement("button");
            planButton.textContent = "планирование задач";
            planButton.onclick = () => taskPlanner(employee.employeeId); 
            employeeDiv.appendChild(planButton);

               

            if (isBoss || currentUserId === employee.employeeId)
            {
                const salaryButton = document.createElement("button");
                salaryButton.textContent = "ЗП архив | прогноз";
                salaryButton.onclick = () => loadSalary(employee.employeeId);
                employeeDiv.appendChild(salaryButton);
            }

            // Контейнер для проектов сотрудника
            const projectList = document.createElement("div");
            projectList.className = "master_list red"; // Класс можно менять в зависимости от статуса
            employee.projects.forEach(project => {
                const projectDiv = document.createElement("div");
                projectDiv.className = "project";

                const projectImage = document.createElement("img");
                // var path = project.sketchPath;
                // var correctedPath = path.Replace("\\\\", "\\");
                projectImage.src =   project.sketchPath ? project.sketchPath : "images/default.jpg";
                projectImage.alt = project.projectName;
                projectDiv.appendChild(projectImage);

                const projectText = document.createElement("div");
                projectText.className = "project_text";

                const projectName = document.createElement("b");
                projectName.textContent = `${project.projectName}`;
                projectText.appendChild(projectName);

                const projectQuantity = document.createElement("p");
                projectQuantity.textContent = `Тираж: ${project.quantity}`;
                projectText.appendChild(projectQuantity);

                const projectDate = document.createElement("p");
                projectDate.textContent = `Дата: ${project.deadline}`;
                projectText.appendChild(projectDate);

                //const projectMaterial = document.createElement("p");
                //projectMaterial.textContent = `Материал: ${project.material}`;
                //projectText.appendChild(projectMaterial);

                projectDiv.appendChild(projectText);
                projectDiv.onclick = () => openProjectCard(project.projectId);
                projectList.appendChild(projectDiv);
            });

            // Добавляем сотрудника и его проекты в контейнер
            employeesContainer.appendChild(employeeDiv);
            employeesContainer.appendChild(projectList);
        });
    } catch (error) {
        console.error("Ошибка при загрузке данных:", error);
    }
}


$('#addProjectButton').click(function () {
    $('#addProject').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');

    $('.popup_before').click(function () {
        $('#addProject').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });

    $.ajax({
        url: `/CurrentProjects/CreateProjectPartial`,
        type: 'GET',      
        success: async function (data) {
            await $('#addProject').html(data);
            getColors(document.getElementById('select-Colors'));
            getMaterials(document.getElementById('select-Materials'));
            getEmployees(document.getElementById('select-Employees'));
            getClients(document.getElementById('select-Clients'));
        }
    });
});

///-------------------------------------планнер---------------------------------------------//

let EmployeeId = 0;
const calendar = document.getElementById("calendar");
const calendarContainer = document.getElementById("calendarContainer");
const taskModal = document.getElementById("taskModal");
const modalTitle = document.getElementById("modalTitle");
const saveTaskButton = document.getElementById("saveTask");
const deleteTaskButton = document.getElementById("deleteTask");
const projectSelect = document.getElementById("taskProjectsSelect");


const taskText = document.getElementById("taskText");
let tasks = []; 
let currentDate = new Date(); 
let loadedMonths = new Set(); 

async function taskPlanner(id) {
    console.log("taskplanner open div");
    calendar.innerHTML = '';
    loadedMonths.clear();

    $('#calendarContainer').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');
    $('.popup_before').click(function () {
        $('#calendarContainer').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });
    const response = await fetch(`/GetTasksByEmployeeId?id=${id}`);
    if (response.ok) {
        let data =await response.json();
        //console.log(data);
        tasks = [];
        data.forEach(d => {
            tasks.push(d);
        }); 
    }
    EmployeeId = id;
    loadInitialMonths();
}
function getDayOfWeekAdjusted(date){
    const day = date.getDay();
    return day === 0 ? 6 : day - 1;
}
function formatDate(year, month, day) {
    const formattedMonth = month.toString().padStart(2, "0"); 
    const formattedDay = day.toString().padStart(2, "0");   
    return `${year}-${formattedMonth}-${formattedDay}`;
}
function generateMonth(date) {
    const monthKey = `${date.getFullYear()}-${date.getMonth()}`;
    if (!loadedMonths.has(monthKey))
        loadedMonths.add(monthKey);
    else return;  

    const start = new Date(date.getFullYear(), date.getMonth(), 1);
    const end = new Date(date.getFullYear(), date.getMonth() + 1, 0);
    const weeks = Math.ceil((getDayOfWeekAdjusted(start) + end.getDate()) / 7);

    const monthHeader = document.createElement("div");
    monthHeader.className = "month-header";
    monthHeader.innerHTML = `<h2>${date.toLocaleString("default", {
        month: "long",
        year: "numeric",
    })}</h2>`;
    calendar.appendChild(monthHeader);

    for (let week = 0; week < weeks; week++) {
        const row = document.createElement("div");
        row.className = "calendar-row";

        for (let day = 0; day < 7; day++) {
            const cell = document.createElement("div");
            cell.className = "empty-calendar-cell";

            const dayNumber = week * 7 + day - getDayOfWeekAdjusted(start) + 1;
            if (dayNumber > 0 && dayNumber <= end.getDate()) {
                cell.className = "calendar-cell";
               
                if (day == 6 || day == 5) {
                    cell.className = "weekend";
                }
                        
                const today = new Date();
                today.setHours(0, 0, 0, 0); 

                const currentDay = new Date(date.getFullYear(), date.getMonth(), dayNumber);
              
                if (currentDay.getTime() === today.getTime())
                {            
                    cell.classList.add("today");
                }
                const taskDate = formatDate(date.getFullYear(), date.getMonth() + 1, dayNumber);
                cell.setAttribute("data-date", taskDate);
                cell.innerHTML = `<div>${dayNumber}</div>`;
                const addButton = document.createElement("button");
                addButton.className = "add-task-btn";
                addButton.innerHTML = "+";
                addButton.onclick = () => openTaskModal( taskDate);
                cell.appendChild(addButton);           
                const dayTasks = tasks.filter((task) => task.deadline === taskDate);                
                for (const task of dayTasks) {             
                    const taskDiv = createTaskDiv(task);
                    taskDiv.classList.add("scrollable-content");
                    cell.appendChild(taskDiv);
                }
            }
            row.appendChild(cell);
        }
        calendar.appendChild(row);
    }       
} 
function loadInitialMonths() {
    const currentMonth = new Date(currentDate.getFullYear(), currentDate.getMonth());
   // generateMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() - 1));
    generateMonth(currentMonth);
    generateMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 1));
}
function createTaskDiv(task) {
    const taskDiv = document.createElement("div");
    taskDiv.className = "task-item";
    taskDiv.style.backgroundColor = task.projectColor;
    taskDiv.style.padding = "5px";
    taskDiv.setAttribute("data-task-id", task.taskId);
    taskDiv.style.margin = "1px";
    taskDiv.style.borderRadius = "5px";
    taskDiv.style.borderStyle = "solid";
    taskDiv.style.borderWidth = "1px";
    taskDiv.style.cursor = "pointer";
    taskDiv.innerHTML = task.description;
    taskDiv.onclick = (e) => {
        e.stopPropagation();
        openTaskModal(task.deadline, task);
    };
    return taskDiv;
}
function addTaskToCalendar(task) {
    const taskDiv = createTaskDiv(task);  
    const cell = document.querySelector(`[data-date="${task.deadline}"]`);  
    if (cell) {
        cell.appendChild(taskDiv);
    }
}
function removeTaskFromCalendar(taskId) {
    const taskDiv = document.querySelector(`[data-task-id="${taskId}"]`);
    if (taskDiv) {
        taskDiv.remove();
    }
}
function openTaskModal(date, task = null) {
    taskModal.style.display = "flex";
    //taskModal.setAttribute("data-date", date);
    let select = document.getElementById("taskProjectsSelect");
    select.innerHTML = '';
    let option = document.createElement("option");
    option.text = "задача без проекта";
    option.value = 0;
    select.append(option);

    const taskIdInput = document.getElementById("taskIdInput");
    taskIdInput.value = 0;
    const taskDate = document.getElementById("taskDate");
    const taskEmployeeIdInput = document.getElementById("taskEmployeeIdInput");


    console.log(" openTaskModal__ EmployeeId  " + EmployeeId);
    taskEmployeeIdInput.value = EmployeeId;
    taskDate.value = date;


    fetch(`/GetCurrentProjectsByEmployeeId?id=${EmployeeId}`)
        .then(response => response.json())
        .then(data => {
           // console.log(data);
            data.forEach(d => {
                let option = document.createElement("option");
                option.text = d.projectName;
                option.value = d.projectId;
                if (task) {
                    if (task.projectId == option.value) {
                        option.selected = true;
                    }
                }
                select.append(option);
            });
        });

    if (task) {
        modalTitle.textContent = "Редактировать задачу";
        taskIdInput.value = task.taskId;
        taskText.value = task.description;
       // projectSelect.value = task.projectId;
        deleteTaskButton.style.display = "inline-block";
        //deleteTaskButton.onclick = () => deleteTask(task);
    } else {
        modalTitle.textContent = "Добавить задачу";
        taskText.value = "";
        projectSelect.selectedIndex = 0;
        deleteTaskButton.style.display = "none";
    }
}

document.getElementById("taskForm").addEventListener("submit", async function (event) {
    event.preventDefault();

    const formData = new FormData(this);
    formData.forEach((value, key) => {
        console.log(key, value);
    });
    const response = await fetch('/CurrentProjects/AddTask', {
        method: 'POST',
        body: formData
    });
    if (response.ok) {
        const task = await response.json();
        const existingTaskIndex = tasks.findIndex(t => t.taskId === task.taskId);

        if (existingTaskIndex !== -1) {
            removeTaskFromCalendar(tasks[existingTaskIndex].taskId);
            tasks.splice(existingTaskIndex, 1);
        }
        tasks.push(task);
        addTaskToCalendar(task);
        closeTaskModal();
    }
});
function closeTaskModal() {
    taskModal.style.display = "none";
    taskText.value = "";
}

document.addEventListener("DOMContentLoaded", () => {
    const deleteTaskButton = document.getElementById("deleteTask");

    if (deleteTaskButton) {
        deleteTaskButton.addEventListener("click", (event) => {
            event.preventDefault(); 
            const taskId = document.getElementById("taskIdInput").value;
            if (taskId) {
                deleteTask(taskId);
            } else {
                console.error("ID задачи отсутствует!");
            }
        });
    }
});
async function deleteTask(id) {
    tasks = tasks.filter((t) => t.taskId !== id);
    const response = await fetch(`/CurrentProjects/DeleteTask?id=${id}`);
    if (response.ok) {
        console.log("task deleted");
        removeTaskFromCalendar(id);
        closeTaskModal();
    }
    else {
        console.log("task delete error");
    }
}

// Рендеринг календаря
//function renderCalendar() {
//  /*  calendar.innerHTML = "";*/
//    const dates = Array.from(loadedMonths).map((key) => {
//        const [year, month] = key.split("-").map(Number);
//        return new Date(year, month);
//    });
//    for (let month of loadedMonths) {
//        console.log(month);
//    }
//    dates.sort((a, b) => a - b);
//    dates.forEach((date) => generateMonth(date));
//}
 
calendarContainer.addEventListener("scroll", () => {
    //if (calendarContainer.scrollTop === 0) {
    //    console.log("scrollTop");
    //    const firstLoadedMonth = Array.from(loadedMonths).sort()[0];
    //    const [year, month] = firstLoadedMonth.split("-").map(Number);
    //    generateMonth(new Date(year, month - 1));
    //   // renderCalendar();
    //} else
    if (calendarContainer.scrollHeight - calendarContainer.scrollTop - 1 <= calendarContainer.clientHeight) {
            const lastLoadedMonth = Array.from(loadedMonths).sort().pop();
            const [year, month] = lastLoadedMonth.split("-").map(Number);
            generateMonth(new Date(year, month + 1));
    }
});

taskModal.addEventListener("click", (e) => {
    if (e.target === taskModal) closeTaskModal()
});


///---------------------------------------галерея---------------------------------------//



let galleryImages = [];
let currentImageIndex = 0;
$('.sketches img').click(function () {
    $('#gallery').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');

    $('.popup_before').click(function () {
        $('#gallery').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });
    //$(this).className.

});
document.addEventListener('DOMContentLoaded', () => {
    const galleries = document.querySelectorAll('.gallery');
    let currentImageIndex = 0;
    let currentGallery = null;

    // Создаём полноэкранную галерею
    const fullscreenOverlay = document.createElement('div');
    fullscreenOverlay.classList.add('fullscreen-overlay');
    fullscreenOverlay.innerHTML = `
        <span class="close-btn">&times;</span>
        <span class="prev-btn">&lt;</span>
        <span class="next-btn">&gt;</span>
        <img src="" alt="">
    `;
    document.body.appendChild(fullscreenOverlay);

    const fullscreenImage = fullscreenOverlay.querySelector('img');
    const closeButton = fullscreenOverlay.querySelector('.close-btn');
    const prevButton = fullscreenOverlay.querySelector('.prev-btn');
    const nextButton = fullscreenOverlay.querySelector('.next-btn');

    // Функция открытия изображения
    function openFullscreen(gallery, index) {
        currentGallery = gallery;
        currentImageIndex = index;
        const images = gallery.querySelectorAll('img');

        console.log(images);

        fullscreenImage.src = images[index].src;
        fullscreenOverlay.classList.add('active');
    }

    // Функция закрытия галереи
    function closeFullscreen() {
        fullscreenOverlay.classList.remove('active');
    }

    // Функция переключения изображений
    function changeImage(direction) {
        if (!currentGallery) return;

        const images = currentGallery.querySelectorAll('img');
        currentImageIndex = (currentImageIndex + direction + images.length) % images.length;
        fullscreenImage.src = images[currentImageIndex].src;
    }

    // Обработчики событий
    galleries.forEach(gallery => {
        gallery.addEventListener('click', (e) => {

            //console.log("gallery click");

            if (e.target.tagName === 'IMG') {

                console.log("gallery click IMG");
                //console.log(e.target);

                const images = [...gallery.querySelectorAll('img')];
                const index = images.indexOf(e.target);
                openFullscreen(gallery, index);
            }
        });
    });

    closeButton.addEventListener('click', closeFullscreen);
    prevButton.addEventListener('click', () => changeImage(-1));
    nextButton.addEventListener('click', () => changeImage(1));
    fullscreenOverlay.addEventListener('click', (e) => {
        if (e.target === fullscreenOverlay) closeFullscreen();
    });

    document.addEventListener('keydown', (e) => {
        if (!fullscreenOverlay.classList.contains('active')) return;

        if (e.key === 'Escape') closeFullscreen();
        if (e.key === 'ArrowLeft') changeImage(-1);
        if (e.key === 'ArrowRight') changeImage(1);
    });

    function openGallery(image) {
        // Находим родительский контейнер (например, .gallery)
        var gallery = image.closest('.gallery');

        // Получаем все изображения внутри родительского контейнера
        var images = gallery.querySelectorAll('img');

        //// Массив с путями к полноразмерным изображениям (например, заменим 'small' на 'large' в путях)
        //var imagePaths = Array.from(images).map(function (img) {
        //    return img.src.replace('small', 'large'); // Предполагаем, что полноразмерные изображения имеют другой суффикс
        //});

        var imagePaths = Array.from(images).map(function (img) {
            return img.src; // Предполагаем, что полноразмерные изображения имеют другой суффикс
        });

        // Создаём модальное окно
        var modal = document.createElement('div');
        modal.classList.add('modal');
        modal.style.position = 'fixed';
        modal.style.top = 0;
        modal.style.left = 0;
        modal.style.width = '100%';
        modal.style.height = '100%';
        modal.style.backgroundColor = 'rgba(0, 0, 0, 0.8)';
        modal.style.display = 'flex';
        modal.style.alignItems = 'center';
        modal.style.justifyContent = 'center';
        modal.style.zIndex = 1000;

        // Добавляем кнопки для навигации по изображениям
        var prevButton = document.createElement('button');
        prevButton.textContent = '◀';
        prevButton.style.position = 'absolute';
        prevButton.style.left = '20px';
        prevButton.style.fontSize = '2em';
        prevButton.style.color = 'white';
        prevButton.style.backgroundColor = 'transparent';
        prevButton.style.border = 'none';
        prevButton.style.cursor = 'pointer';

        var nextButton = document.createElement('button');
        nextButton.textContent = '▶';
        nextButton.style.position = 'absolute';
        nextButton.style.right = '20px';
        nextButton.style.fontSize = '2em';
        nextButton.style.color = 'white';
        nextButton.style.backgroundColor = 'transparent';
        nextButton.style.border = 'none';
        nextButton.style.cursor = 'pointer';

        // Создаем элемент для отображения изображений
        var modalImage = document.createElement('img');
        modalImage.src = image.src.replace('small', 'large');
        modalImage.style.maxWidth = '90%';
        modalImage.style.maxHeight = '90%';
        modalImage.style.objectFit = 'contain';

        // Добавляем изображение в модальное окно
        modal.appendChild(modalImage);
        modal.appendChild(prevButton);
        modal.appendChild(nextButton);

        // Открываем модальное окно
        document.body.appendChild(modal);

        // Функции для перехода к предыдущему и следующему изображению
        var currentIndex = imagePaths.indexOf(modalImage.src);

        function showImage(index) {
            if (index < 0) {
                index = imagePaths.length - 1; // Если индекс меньше 0, показать последнее изображение
            } else if (index >= imagePaths.length) {
                index = 0; // Если индекс больше длины массива, показать первое изображение
            }
            modalImage.src = imagePaths[index];
            currentIndex = index;
        }

        prevButton.addEventListener('click', function () {
            showImage(currentIndex - 1);
        });

        nextButton.addEventListener('click', function () {
            showImage(currentIndex + 1);
        });

        // Закрытие модального окна при клике на его фон
        modal.addEventListener('click', function (event) {
            if (event.target === modal) {
                document.body.removeChild(modal);
            }
        });

        // Закрытие модального окна при нажатии на клавишу Escape
        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') {
                document.body.removeChild(modal);
            }
        });
    }

});


async function plusClick(fileType, id) {

    // event.stopPropagation();

    $('#addImagePopUp').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');
    $('.popup_before').css('z-index', '3');

    $('#addImagePopUp').find('#addPhotoProjectId').val(id);
    $('#addImagePopUp').find('#addPhotoFileType').val(fileType);
    let select = $('#addImagePopUp').find('#itemSelect');
    select.innerHTML = '';
    let optionFirst = document.createElement("option");
    optionFirst.text = "общие фото";
    optionFirst.value = 0;
    select.append(optionFirst);

    try {
        const response = await fetch(`/GetItemsByProjectId?id=${id}`, {
            method: "GET"
        });

        if (response.ok) {
            let data = await response.json();
           // console.log(data);
            let index = 1;
            data.forEach(i => {
                let option = document.createElement("option");
                if (i.title != null) { option.text = i.title; }
                else { option.text = "item " + index++ }
                option.value = i.id;
                select.append(option);
            });
        }
    } catch (error) {
        console.error("Ошибка при загрузке данных:", error);
    }

    $('.close-btn').click(function () {
        $('#addImagePopUp').css('visibility', 'hidden');
        $('.popup_before').css('z-index', '1');
    });

}

//-----------------------------------отправка формы с фото проекта, журнала---------------------------//
async function setProjectColor(input, id) {
    const color = input.value;
    fetch(`/SetProjectColor?id=${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ color })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Ошибка при отправке данных на сервер");
            }
            return response.json(); // Если требуется ответ от сервера
        })
        .then(data => {
            console.log("Успешно отправлено:", data);
        })
        .catch(error => {
            console.error("Ошибка:", error);
        });
}

function openProjectCard(id) {
    console.log("Открытие карточки проекта с ID:", id);
    $('#projectData').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');

    $.ajax({
        url: `/LoadProjectCard`,
        type: 'GET',
        data: { id: id },
        success: function (data) {
            $('#projectData').html(data);
        },
        error: function () {
            alert("Ошибка при загрузке данных проекта.");
        }
    });

 

    $('.popup_before').click(function () {
        $('#projectData').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });
}

//document.addEventListener("DOMContentLoaded", () => {
//    const textarea = document.getElementsByTagName("textarea");
//    textarea.forEach( t => textareaResize(t)); // Подстроить высоту при загрузке страницы
//});

//не ресайзится изначально суука
function textareaResize(textarea) {
    /*textarea.style.height = "auto"; */
    textarea.style.height = textarea.scrollHeight + "px";
}
function sendCardText(action, projectId) {
    const textarea = event.target;
    const description = textarea.value;
    console.log("sendCardText: " + description);
    console.log(JSON.stringify({ description }));

    fetch(`/${action}?id=${projectId}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ description })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Ошибка при отправке данных на сервер");
            }
            return response.json(); // Если требуется ответ от сервера
        })
        .then(data => {
            console.log("Успешно отправлено:", data);
        })
        .catch(error => {
            console.error("Ошибка:", error);
        });
}


document.getElementById("photoForm").addEventListener("submit", async function (event) {
    event.preventDefault(); 

    const projectId = document.getElementById("addPhotoProjectId").value;
    const fileType = document.getElementById("addPhotoFileType").value;
    const itemPointer = document.getElementById("itemSelect").value;
    const photos = document.querySelector('input[type="file"]').files;

    const formData= new FormData();
    const fileInput = document.getElementById('AddPhotosInput');
    formData.append(`projectId`, projectId);
    formData.append(`fileType`, fileType);
    formData.append(`itemPointer`, itemPointer);
   
    const files = fileInput.files;
    for (let i = 0; i < files.length; i++) {
        const file = files[i];
        formData.append(`photos[${i}].photo`, file);
        formData.append(`photos[${i}].projectId`, projectId);
        formData.append(`photos[${i}].fileType`, fileType);
        formData.append(`photos[${i}].itemPointer`, itemPointer);
    }
    formData.forEach((value, key) => {
        console.log(key, value);
    });

    const itemsResponse = await fetch('/CurrentProjects/AddPhotos', {
        method: 'POST',
        body: formData
    });
    if (itemsResponse.ok) {
        console.log('загружены photos.');
        $('#addImagePopUp').css('visibility', 'hidden');
        $('.popup_before').css('z-index', '1');
    }
});


    //const textarea = event.target;
    
    //console.log("sendCardText: " + description);
    //console.log(JSON.stringify({ description }));

    //fetch(`/${action}?id=${projectId}`, {
    //    method: "PUT",
    //    headers: {
    //        "Content-Type": "application/json"
    //    },
    //    body: JSON.stringify({ description })
    //})
    //    .then(response => {
    //        if (!response.ok) {
    //            throw new Error("Ошибка при отправке данных на сервер");
    //        }
    //        return response.json(); // Если требуется ответ от сервера
    //    })
    //    .then(data => {
    //        console.log("Успешно отправлено:", data);
    //    })
    //    .catch(error => {
    //        console.error("Ошибка:", error);
    //    });


function AddEmployee() {
    // Показываем всплывающее окно
    $('#addEmployeePopUp').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');
    $('.popup_before').css('z-index', '3');

    // Закрытие всплывающего окна при клике на .popup_before
    $('.close-btn').click(function () {
        $('#addEmployeePopUp').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
        $('.popup_before').css('z-index', '1');
    });
}
//-------------------------------------------- Client  -----------------------------------------------||
function AddClient() {
    $('#addClient').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');
    $('.popup_before').css('z-index', '3');
    $('.close-btn').click(function () {
        $('#addClient').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
        $('.popup_before').css('z-index', '1');
    });



}

 function addContactRow (button) {
    // Обработчик для добавления строки контакта
   // const addContactRowButton = document.getElementById('addContactRow');
    const contactsTableBody = document.getElementById('addContactsTable');

  /*  addContactRowButton.addEventListener('click', function () {*/
        const newIndex = contactsTableBody.querySelectorAll('tr').length;
        const newRow = `
            <tr>
                <td><input type="text" name="model.contacts[${newIndex}].Name" class="form-control" placeholder="Имя"></td>
                <td><input type="text" name="model.contacts[${newIndex}].Phone" class="form-control" placeholder="Телефон"></td>
                <td><input type="text" name="model.contacts[${newIndex}].Email" class="form-control" placeholder="Почта"></td>
                  <td>
                    <!-- Кнопка удаления (картинка крестика) -->
                    <button type="button" class="btn btn-danger remove-item">
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-x-circle" viewBox="0 0 16 16">
                            <path d="M11.854 4.146a.5.5 0 0 1 0 .708L8.707 8 11.854 11.146a.5.5 0 1 1-.708.708L8 8.707 4.854 11.854a.5.5 0 1 1-.708-.708L7.293 8 4.146 4.854a.5.5 0 1 1 .708-.708L8 7.293l3.146-3.147a.5.5 0 0 1 .708 0z" />
                        </svg>
                    </button>
                </td>
            </tr>
        `;
        contactsTableBody.insertAdjacentHTML('beforeend', newRow);
        document.querySelectorAll('.remove-item').forEach(but => {
            but.addEventListener('click', function () {
                console.log('remove-item click');
                const row = but.closest('tr');  // Находим строку таблицы (родитель для кнопки)
                row.remove();  // Удаляем строку
            });
        });
 }

function addAddressRow (button) {
    const addressTableBody = document.getElementById('addAddressTable');
   
        const newIndex = addressTableBody.querySelectorAll('tr').length;
        const newRow = `
            <tr>
                <td><input type="text" name="model.address[${newIndex}].Address" class="form-control" placeholder="Адрес"></td>
                  <td>
                    <!-- Кнопка удаления (картинка крестика) -->
                    <button type="button" class="btn btn-danger remove-item">
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-x-circle" viewBox="0 0 16 16">
                            <path d="M11.854 4.146a.5.5 0 0 1 0 .708L8.707 8 11.854 11.146a.5.5 0 1 1-.708.708L8 8.707 4.854 11.854a.5.5 0 1 1-.708-.708L7.293 8 4.146 4.854a.5.5 0 1 1 .708-.708L8 7.293l3.146-3.147a.5.5 0 0 1 .708 0z" />
                        </svg>
                    </button>
                </td>
            </tr>
        `;
        addressTableBody.insertAdjacentHTML('beforeend', newRow);
        document.querySelectorAll('.remove-item').forEach(but => {
            but.addEventListener('click', function () {
                console.log('remove-item click');
                const row = but.closest('tr');  // Находим строку таблицы (родитель для кнопки)
                row.remove();  // Удаляем строку
            });
        });
}

function addRekvizitRow(button) {
    const rekvizitTableBody = document.getElementById('rekvizitTable');

    const newIndex = rekvizitTableBody.querySelectorAll('tr').length;
    const newRow = `
            <tr>
                <td><input type="file" name="model.rekvizits[${newIndex}].FilePath" class="form-control" placeholder="Реквизиты"></td>
                  <td>
                    <!-- Кнопка удаления (картинка крестика) -->
                    <button type="button" class="btn btn-danger remove-item">
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-x-circle" viewBox="0 0 16 16">
                            <path d="M11.854 4.146a.5.5 0 0 1 0 .708L8.707 8 11.854 11.146a.5.5 0 1 1-.708.708L8 8.707 4.854 11.854a.5.5 0 1 1-.708-.708L7.293 8 4.146 4.854a.5.5 0 1 1 .708-.708L8 7.293l3.146-3.147a.5.5 0 0 1 .708 0z" />
                        </svg>
                    </button>
                </td>
            </tr>
        `;
    rekvizitTableBody.insertAdjacentHTML('beforeend', newRow);
    document.querySelectorAll('.remove-item').forEach(but => {
        but.addEventListener('click', function () {
            console.log('remove-item click');
            const row = but.closest('tr');  // Находим строку таблицы (родитель для кнопки)
            row.remove();  // Удаляем строку
        });
    });
}

$('#addClientForm').submit(async function (e) {
    e.preventDefault();
    const formData = new FormData(this);
    formData.forEach((value, key) => {
        console.log(key, value);
    });
    try {
            const response = await fetch('/CurrentProjects/AddClient', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                alert('Информация о клиенте успешно загружена.');
               
               // console.log('Данные клиента успешно загружены.');
            } else {
                const errorText = await response.text();
                console.error('Ошибка при загрузке данных клиента:', errorText);
              //  alert('Ошибка при загрузке данных клиента.');
            }
        } catch (error) {
            console.error('Ошибка при отправке запроса:', error);
          //  alert('Произошла ошибка при отправке данных.');
        }
    //const formDataFiles = new FormData();
    //const fileInput = document.getElementById('ClientRekvizit');
    //const clientTitle = $('#ClientTitle').val();
    //const files = fileInput.files;
    //for (let i = 0; i < files.length; i++) {
    //    const file = files[i];
    //    formDataFiles.append(`rekvizits[${i}].FilePath`, file);
    //    formDataFiles.append(`rekvizits[${i}].ClientTitle`, clientTitle);
    //}
    //console.log("rekvizit");
    //formDataFiles.forEach((value, key) => {
    //    console.log(key, value);
    //});

    // Формируем данные для загрузки файлов реквизитов
    const formDataFiles = new FormData();
    const clientTitle = $('#ClientTitle').val();

    $('#rekvizitTable input[type="file"]').each(function (index, input) {
        const files = input.files;
        for (let i = 0; i < files.length; i++) {
            formDataFiles.append(`rekvizits[${index}].FilePath`, files[i]);
            formDataFiles.append(`rekvizits[${index}].ClientTitle`, clientTitle);
        }
    });

    console.log("rekvizit");
    formDataFiles.forEach((value, key) => {
        console.log(key, value);
    });



    const filesResponse = await fetch('/CurrentProjects/AddRervizit', {
        method: 'POST',
        body: formDataFiles
    });
    if (filesResponse.ok) {
        console.log('файлы загружены.');
        // window.location.href = '';
        $('#addClient').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    }
    if (!filesResponse.ok) {
        throw new Error('Ошибка при загрузке файлов проекта.');
    }
});



//$(document).on('submit', '#editClientForm', async function (e) {
//    e.preventDefault();
//    const formData = new FormData(this);
//    formData.forEach((value, key) => {
//        console.log(key, value);
//    });

//    try {
//        const response = await fetch('/CurrentProjects/EditClient', {
//            method: 'POST',
//            body: formData
//        });

//        if (response.ok) {
//            const message = await response.json();
//            alert(message);
//            console.log('Данные клиента успешно обновлены.');
//        } else {
//            const errorText = await response.text();
//            console.error('Ошибка при загрузке данных клиента:', errorText);
//        }
//    } catch (error) {
//        console.error('Ошибка при отправке запроса:', error);
//    }

//    const formDataFiles = new FormData();
//    const clientTitle = $('#ClientTitle').val();

//    $('#rekvizitTable input[type="file"]').each(function (index, input) {
//        const files = input.files;
//        for (let i = 0; i < files.length; i++) {
//            formDataFiles.append(`rekvizits[${index}].FilePath`, files[i]);
//            formDataFiles.append(`rekvizits[${index}].ClientTitle`, clientTitle);
//        }
//    });

//    console.log("rekvizit");
//    formDataFiles.forEach((value, key) => {
//        console.log(key, value);
//    });

//    const filesResponse = await fetch('/CurrentProjects/AddRervizit', {
//        method: 'POST',
//        body: formDataFiles
//    });

//    if (filesResponse.ok) {
//        console.log('Файлы загружены.');
//        $('#editClient').css('visibility', 'hidden');
//        $('.popup_before').css('visibility', 'hidden');
//    } else {
//        console.error('Ошибка при загрузке файлов проекта.');
//    }
//});


async function editClientDivOpen(id) {
    let div = $('#editClient');
    $('#editClient').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');
    $('.popup_before').css('z-index', '3');
    // Загрузка _CreateClientPartial с передачей client
    await $.get('/CurrentProjects/EditClientPartial', { clientId: id }, function (data) {
        div.html(data); // Вставляем HTML из представления в div
    });
    $('#editClientForm').submit(async function (e) {
        e.preventDefault();
        const formData = new FormData(this);
        formData.forEach((value, key) => {
            console.log(key, value);
        });

        try {
            const response = await fetch('/CurrentProjects/EditClient', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const message = await response.json();
                alert(message);

                console.log('Данные клиента успешно обновлены.');
            } else {
                const errorText = await response.text();
                console.error('Ошибка при загрузке данных клиента:', errorText);
                //  alert('Ошибка при загрузке данных клиента.');
            }
        } catch (error) {
            console.error('Ошибка при отправке запроса:', error);
            //  alert('Произошла ошибка при отправке данных.');
        }

        //const formDataFiles = new FormData();
        //const clientTitle = $('#ClientTitle').val();
      
        //$('#rekvizitTable input[type="file"]').each(function (index, input) {
        //    const files = input.files;
        //        for (let i = 0; i < files.length; i++) {
        //            formDataFiles.append(`rekvizits[${index}].FilePath`, files[i]);
        //            formDataFiles.append(`rekvizits[${index}].ClientTitle`, clientTitle);
        //        }
        //    });

        //console.log("rekvizit");
        //formDataFiles.forEach((value, key) => {
        //    console.log(key, value);
        //});


        const files = document.querySelector('#rekvizitTable input[type="file"]').files;
        const formDataFiles = new FormData();
        const clientTitle = $('#ClientTitle').val();
       
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            formDataFiles.append(`rekvizits[${i}].FilePath`, file);
            formDataFiles.append(`rekvizits[${i}].ClientTitle`, clientTitle);
          
        }

        console.log("rekvizit");
        formDataFiles.forEach((value, key) => {
            console.log(key, value);
        });

        const filesResponse = await fetch('/CurrentProjects/AddRervizit', {
            method: 'POST',
            body: formDataFiles
        });
        if (filesResponse.ok) { 
            console.log('файлы загружены.');
            // window.location.href = '';
            $('#editClient').css('visibility', 'hidden');
            $('.popup_before').css('visibility', 'hidden');
        }
        if (!filesResponse.ok) {
            throw new Error('Ошибка при загрузке файлов проекта.');
        }
    });
    $('.close-btn').click(function () {
        $('#editClient').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
        $('.popup_before').css('z-index', '1');
    });
    $('.popup_before').click(function () {
        $('#editClient').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
        $('.popup_before').css('z-index', '1');
    });
}
function deleteContact(id, but) {
    if (confirm("Точно удалить этот контакт?")) {
        console.log("Удалить контакт : " + id);
        $.ajax({
            url: `/CurrentProjects/DeleteContact`,
            type: 'POST',
            data: { id: id },
            success: function () {
                alert('Потрачено');
                const row = but.closest('tr'); 
                row.remove();  
            },
            error: function () {
                alert('Ошибка');
            }
        });
    }
}
function deleteAddress(id, but) {
    if (confirm("Точно удалить этот адрес?")) {
        console.log("Удалить адрес : " + id);
        $.ajax({
            url: `/CurrentProjects/DeleteAddress`,
            type: 'POST',
            data: { id: id },
            success: function () {
                alert('Потрачено');
                const row = but.closest('tr');
                row.remove();  
            },
            error: function () {
                alert('Ошибка');
            }
        });
    }
}
function deleteRekvizit(id, but) {
    if (confirm("Точно удалить эти реквизиты?")) {
        console.log("Удалить реквизиты : " + id);
        $.ajax({
            url: `/CurrentProjects/DeleteRekvizit`,
            type: 'POST',
            data: { id: id },
            success: function () {
                alert('Потрачено');
                const row = but.closest('tr');
                row.remove();  
               // location.reload();
            },
            error: function () {
                alert('Ошибка');
            }
        });
    }
}

function deleteClient(id) {
    if (confirm("Точно удалить заказчика?")) {
        console.log("Удалить заказчика : " + id);
        $.ajax({
            url: `/CurrentProjects/DeleteClient`,
            type: 'POST',
            data: { id: id },
            success: function () {
                alert('Потрачено');       
                // location.reload();
            },
            error: function () {
                alert('Ошибка');
            }
        });
    }
}

//-------------------------------------------- Employee -----------------------------------------------||


async function loadEmployeeCard(id) {
    console.log("Открытие карточки сотрудника ID:", id);

    $('#employeeData').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');

    $.ajax({
        url: `/CurrentProjects/LoadEmployeeCard`,
        type: 'GET',
        data: { id: id },
        success: async function (data) {
          
            await $('#employeeData').html(data);
        }
    });

    $('.popup_before').click(function () {
        $('#employeeData').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });
}

//document.querySelector('form').addEventListener('submit', async (event) => {
//    event.preventDefault(); // Остановить стандартное поведение формы

//    const form = event.target;
//    const formData = new FormData(form);

//    try {
//        const response = await fetch(form.action, {
//            method: form.method,
//            body: formData
//        });

//        if (response.ok) {
//            alert("Сотрудник и пользователь успешно добавлены.");
//            form.reset(); // Очистка формы
//        } else if (response.status === 409) {
//            const error = await response.json();
//            alert(error.Message); // Показываем сообщение о конфликте
//        } else {
//            const error = await response.json();
//            alert("Ошибка: " + error.Message); // Другие ошибки
//        }
//    } catch (error) {
//        alert("Произошла ошибка при отправке данных.");
//        console.error(error);
//    }
//});


//document.querySelector('#AddEmployeeForm').addEventListener('submit', function (event) {
//        event.preventDefault();  // Отменяем стандартную отправку формы, чтобы выводить данные в консоль

//        const formData = new FormData(this);
//        const formEntries = {};

//        // Преобразуем FormData в обычный объект для более удобного вывода
//        formData.forEach((value, key) => {
//            formEntries[key] = value;
//        });

//        console.log('Данные формы:', formEntries);

//        // После логирования данных, можно отправить форму, если нужно
//         this.submit();
//    });


async function fireEmployee(id) {
    if (confirm("Точно уволить?")) {
        console.log("Fire employee : " + id);
        $.ajax({
            url: `/CurrentProjects/FireEmployee`,
            type: 'POST',
            data: { id: id },
            success: function () {
                alert('Уволено');
                location.reload();
            },
            error: function () {
                alert('Ошибка');
            }
        });
    }

}

//--------------------------------------------методы странички CreateProject-----------------------------------------------||



//document.getElementById('addItem').addEventListener('click', function () {
//    const tableBody = document.querySelector('#addItemTable');
//    const rowCount = tableBody.querySelectorAll('tr').length;

//    // Создаём новую строку
//    let tr = document.createElement('tr');

//    // Столбец: Тип элемента
//    let td1 = document.createElement('td');
//    let itemTypeSelect = document.createElement('select');
//    itemTypeSelect.name = `items[${rowCount}].itemType`;
//    itemTypeSelect.classList.add('form-select');
//    itemTypeSelect.innerHTML = `
//        <option selected value="Медаль">Медаль</option>
//        <option value="Статуэтка">Статуэтка</option>
//        <option value="Брелок">Брелок</option>`;
//    td1.appendChild(itemTypeSelect);
//    tr.appendChild(td1);

//    let tdName = document.createElement('td');
//    let itemNameInput = document.createElement('input');
//    itemNameInput.type = 'text';
//    itemNameInput.name = `items[${rowCount}].itemName`;
//    itemNameInput.classList.add('form-control');
//    tdName.appendChild(itemNameInput);
//    tr.appendChild(tdName);

//    // Столбец: Поле загрузки файла
//    let td2 = document.createElement('td');
//    let fileInput = document.createElement('input');
//    fileInput.type = 'file';
//    fileInput.name = `items[${rowCount}].sketch`;
//    fileInput.classList.add('form-control');
//    td2.appendChild(fileInput);
//    tr.appendChild(td2);

//    // Столбец: Превью изображения
//    let td3 = document.createElement('td');
//    let img = document.createElement('img');
//    img.src = '';
//    img.style.width = '100px';
//    img.style.height = 'auto';
//    td3.appendChild(img);
//    tr.appendChild(td3);

//    // Обновляем превью изображения при выборе файла
//    fileInput.addEventListener('change', function () {
//        const file = fileInput.files[0];
//        if (file) {
//            const reader = new FileReader();
//            reader.onload = function (e) {
//                img.src = e.target.result;
//            };
//            reader.readAsDataURL(file);
//        }
//    });

//    // Столбец: Количество
//    let td4 = document.createElement('td');
//    let itemCountInput = document.createElement('input');
//    itemCountInput.type = 'number';
//    itemCountInput.name = `items[${rowCount}].itemCount`;
//    itemCountInput.classList.add('form-control');
//    td4.appendChild(itemCountInput);
//    tr.appendChild(td4);

//    // Столбец: Дата завершения
//    let td5 = document.createElement('td');
//    let itemDeadlineInput = document.createElement('input');
//    itemDeadlineInput.type = 'date';
//    itemDeadlineInput.name = `items[${rowCount}].itemDeadline`;
//    itemDeadlineInput.classList.add('form-control');
//    td5.appendChild(itemDeadlineInput);
//    tr.appendChild(td5);

//    // Столбец: Цена
//    let td6 = document.createElement('td');
//    let itemPriceInput = document.createElement('input');
//    itemPriceInput.type = 'number';
//    itemPriceInput.name = `items[${rowCount}].itemPrice`;
//    itemPriceInput.classList.add('form-control');
//    td6.appendChild(itemPriceInput);
//    tr.appendChild(td6);

//    // Столбец: Материалы
//    let td7 = document.createElement('td');
//    let materialsSelect = document.createElement('select');
//    materialsSelect.name = `items[${rowCount}].materials`;
//    materialsSelect.id = 'itemMaterials';
//    materialsSelect.multiple = true;
//    getMaterials(materialsSelect);
//    td7.appendChild(materialsSelect);
//    tr.appendChild(td7);

//    // Столбец: Цвета
//    let td8 = document.createElement('td');
//    let colorsSelect = document.createElement('select');
//    colorsSelect.name = `items[${rowCount}].colors`;
//    colorsSelect.id = 'itemColors';
//    colorsSelect.multiple = true;
//    getColors(colorsSelect);
//    td8.appendChild(colorsSelect);
//    tr.appendChild(td8);

//    // Столбец: Описание
//    let td9 = document.createElement('td');
//    let itemDescriptionInput = document.createElement('input');
//    itemDescriptionInput.type = 'text';
//    itemDescriptionInput.name = `items[${rowCount}].itemDescription`;
//    itemDescriptionInput.classList.add('form-control');
//    td9.appendChild(itemDescriptionInput);
//    tr.appendChild(td9);

//    let td10 = document.createElement('td');
//    let deleteButton = document.createElement('button');
//    deleteButton.innerHTML = ` <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-x-circle" viewBox="0 0 16 16">
//    <path d="M11.854 4.146a.5.5 0 0 1 0 .708L8.707 8 11.854 11.146a.5.5 0 1 1-.708.708L8 8.707 4.854 11.854a.5.5 0 1 1-.708-.708L7.293 8 4.146 4.854a.5.5 0 1 1 .708-.708L8 7.293l3.146-3.147a.5.5 0 0 1 .708 0z" />
//                    </svg > `;
//    deleteButton.addEventListener('click', function () {
//        tableBody.removeChild(tr);
//    });
//    td10.appendChild(deleteButton);
//    tr.appendChild(td10);

//    // Добавляем строку в таблицу
//    tableBody.appendChild(tr);
//});

 function addItemRow () {
    const tableBody = document.querySelector('#addItemTable');
    const rowCount = tableBody.querySelectorAll('tr').length;

    // Создаём новую строку
    let tr = document.createElement('tr');

    // Столбец: Тип элемента
    let td1 = document.createElement('td');
    let itemTypeSelect = document.createElement('select');
    itemTypeSelect.name = `items[${rowCount}].itemType`;
    itemTypeSelect.classList.add('form-select');
    itemTypeSelect.innerHTML = `
        <option selected value="Медаль">Медаль</option>
        <option value="Статуэтка">Статуэтка</option>
        <option value="Брелок">Брелок</option>`;
    td1.appendChild(itemTypeSelect);
    tr.appendChild(td1);

    //let tdName = document.createElement('td');
    //let itemNameInput = document.createElement('input');
    //itemNameInput.type = 'text';
    //itemNameInput.name = `items[${rowCount}].itemName`;
    //itemNameInput.classList.add('form-control');
    //tdName.appendChild(itemNameInput);
    //tr.appendChild(tdName);

    // Столбец: Поле загрузки файла
    let td2 = document.createElement('td');
    let fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.name = `items[${rowCount}].sketch`;
    fileInput.classList.add('form-control');
    td2.appendChild(fileInput);
    tr.appendChild(td2);

    // Столбец: Превью изображения
    let td3 = document.createElement('td');
    let img = document.createElement('img');
    img.src = '';
    img.style.width = '100px';
    img.style.height = 'auto';
    td3.appendChild(img);
    tr.appendChild(td3);

    // Обновляем превью изображения при выборе файла
    fileInput.addEventListener('change', function () {
        const file = fileInput.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                img.src = e.target.result;
            };
            reader.readAsDataURL(file);
        }
    });

    // Столбец: Количество
    let td4 = document.createElement('td');
    let itemCountInput = document.createElement('input');
    itemCountInput.type = 'number';
    itemCountInput.name = `items[${rowCount}].itemCount`;
    itemCountInput.classList.add('form-control');
    td4.appendChild(itemCountInput);
    tr.appendChild(td4);

    // Столбец: Дата завершения
    let td5 = document.createElement('td');
    let itemDeadlineInput = document.createElement('input');
    itemDeadlineInput.type = 'date';
    itemDeadlineInput.name = `items[${rowCount}].itemDeadline`;
    itemDeadlineInput.classList.add('form-control');
    td5.appendChild(itemDeadlineInput);
    tr.appendChild(td5);

    // Столбец: Цена
    let td6 = document.createElement('td');
    let itemPriceInput = document.createElement('input');
    itemPriceInput.type = 'number';
    itemPriceInput.name = `items[${rowCount}].itemPrice`;
    itemPriceInput.classList.add('form-control');
    td6.appendChild(itemPriceInput);
    tr.appendChild(td6);

    // Столбец: Материалы
    let td7 = document.createElement('td');
    let materialsSelect = document.createElement('select');
    materialsSelect.name = `items[${rowCount}].materials`;
    materialsSelect.id = 'itemMaterials';
    materialsSelect.multiple = true;
    getMaterials(materialsSelect);
     td7.appendChild(materialsSelect);

     let addMaterialButton = document.createElement('button');
     addMaterialButton.type = 'button';
     addMaterialButton.innerText = '+';
     addMaterialButton.onclick = function () { showInput(`materials-${rowCount}`); };
     td7.appendChild(addMaterialButton);

     let materialInputDiv = document.createElement('div');
     materialInputDiv.id = `input-materials-${rowCount}`;
     materialInputDiv.style.display = 'none';
     materialInputDiv.innerHTML = `
        <input type="text" id="new-Materials-${rowCount}" placeholder="Введите название" />
        <button type="button" onclick="addOption('Materials', this)">Добавить</button>`;
     td7.appendChild(materialInputDiv);
    tr.appendChild(td7);

    // Столбец: Цвета
    let td8 = document.createElement('td');
    let colorsSelect = document.createElement('select');
    colorsSelect.name = `items[${rowCount}].colors`;
    colorsSelect.id = 'itemColors';
    colorsSelect.multiple = true;
    getColors(colorsSelect);
    td8.appendChild(colorsSelect);
     let addColorButton = document.createElement('button');
     addColorButton.type = 'button';
     addColorButton.innerText = '+';
     addColorButton.onclick = function () { showInput(`Colors-${rowCount}`); };
     td8.appendChild(addColorButton);

     let colorInputDiv = document.createElement('div');
     colorInputDiv.id = `input-Colors-${rowCount}`;
     colorInputDiv.style.display = 'none';
     colorInputDiv.innerHTML = `
        <input type="text" id="new-Colors-${rowCount}" placeholder="Введите название" />
        <button type="button" onclick="addOption('Colors', this)">Добавить</button>`;
     td8.appendChild(colorInputDiv);

    tr.appendChild(td8);

    // Столбец: Описание
    let td9 = document.createElement('td');
    let itemDescriptionInput = document.createElement('input');
    itemDescriptionInput.type = 'text';
    itemDescriptionInput.name = `items[${rowCount}].itemDescription`;
    itemDescriptionInput.classList.add('form-control');
    td9.appendChild(itemDescriptionInput);
    tr.appendChild(td9);

    let td10 = document.createElement('td');
    let deleteButton = document.createElement('button');
    deleteButton.innerHTML = ` <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-x-circle" viewBox="0 0 16 16">
    <path d="M11.854 4.146a.5.5 0 0 1 0 .708L8.707 8 11.854 11.146a.5.5 0 1 1-.708.708L8 8.707 4.854 11.854a.5.5 0 1 1-.708-.708L7.293 8 4.146 4.854a.5.5 0 1 1 .708-.708L8 7.293l3.146-3.147a.5.5 0 0 1 .708 0z" />
                    </svg > `;
    deleteButton.addEventListener('click', function () {
        tableBody.removeChild(tr);
    });
    td10.appendChild(deleteButton);
    tr.appendChild(td10);

    // Добавляем строку в таблицу
    tableBody.appendChild(tr);
}

//document.querySelectorAll('.remove-item').forEach(button => {
//    button.addEventListener('click', function () {
//        const row = button.closest('tr');  
//        row.remove();  
//    });
//});

function removeItemRow(button) {
        const row = button.closest('tr'); 
        row.remove(); 
    }

document.querySelector('#fileInputFirst').addEventListener('change', function () {
    const file = this.files[0];
    if (file) {
        const reader = new FileReader();
        const img = document.querySelector('#imgFirst');
        reader.onload = function (e) {
            img.src = e.target.result;
        };
        reader.readAsDataURL(file);
    }
});
function showInput(selectId) {
    const inputContainer = document.getElementById(`input-${selectId}`);
    inputContainer.style.display = 'block'; // Показать блок с инпутом
}

//document.addEventListener('DOMContentLoaded', function () {
//    getColors(document.getElementById('select-Colors'));
//    getMaterials(document.getElementById('select-Materials'));
//    getEmployees(document.getElementById('select-Employees'));
//    getClients(document.getElementById('select-Clients'));
//});
async function getMaterials(select) {
    console.log(select);
    try {
        const response = await fetch('/CurrentProjects/GetMaterials'); // Replace 'YourController' with the actual controller name
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const materials = await response.json();
        select.innerHTML = '';
        materials.forEach(material => {
            const option = document.createElement('option');
           
            option.textContent = material.materialName;
            select.appendChild(option);
        });
    } catch (error) {
        console.error('Error loading materials:', error);
    }
}
async function getColors(select) {
   // const select = document.getElementById('select-Colors');
    try {
        const response = await fetch('/CurrentProjects/GetColors'); // Replace 'YourController' with the actual controller name
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const colors = await response.json();
        select.innerHTML = '';
        colors.forEach(color => {
            const option = document.createElement('option');
           
            option.textContent = color.colorName;
            select.appendChild(option);
        });
    } catch (error) {
        console.error('Error loading colors:', error);
    }
}
async function getClients(select) {
    try {
        const response = await fetch('/CurrentProjects/GetClients');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const clients = await response.json();
        select.innerHTML = '';
        clients.forEach(client => {
            const option = document.createElement('option');
           
            option.textContent = client.title;
            option.value = client.clientId;
            select.appendChild(option);
        });
    } catch (error) {
        console.error('Error loading colors:', error);
    }
}
async function getEmployees(select) {
    try {
        const response = await fetch('/CurrentProjects/GetEmployees');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const employees = await response.json();
       /* select.innerHTML = '';*/
        employees.forEach(employee => {
            const option = document.createElement('option');
           
            option.textContent = employee.employeeName;
            option.value = employee.employeeId;
            select.appendChild(option);
        });
    } catch (error) {
        console.error('Error loading colors:', error);
    }
}
async function addOption(selectId, button) {
    console.log("addOption select-" + selectId);
   

    const td = button.closest('td');
    const select = td.querySelector(`select`);
    const input = td.querySelector(`input`);
     
    const value = input.value.trim();

    if (value) {
        const option = document.createElement('option');
        option.text = value;
        option.value = value;
        select.add(option); // Добавляем новый option в select
        input.value = ''; // Очищаем инпут
        input.parentElement.style.display = 'none'; // Скрыть инпут после добавления
        const itemsResponse = await fetch('/CurrentProjects/Add' + selectId + '?' + selectId + '=' + value, {
            method: 'POST'
        });
        if (itemsResponse.ok) {
            console.log('загружены items.');
        }
    } else {
        alert('Введите корректное название!');
    }
}
//-----------------------------------------проект-------------------------------------//
async function takeProject(employeeId, projectId) {
    if (confirm("Вы уверены, что хотите назначить мастера?")) {
        $.ajax({
            url: `/CurrentProjects/TakeProject?employeeId=${employeeId}&projectId=${projectId}`,
            type: 'POST',
            success: function () {
                alert('мастер назначен');
                location.reload();
            },
            error: function () {
                alert('Ошибка переназначения');
            }
        });
    }
}
async function deleteItem(id) {
    if (confirm("Вы уверены, что хотите удалить этот Item?")) {
        $.ajax({
            url: `/CurrentProjects/DeleteItem?id=${id}`,
            type: 'POST',
            success: function () {
                alert('item успешно удалён');
                location.reload();
            },
            error: function () {
                alert('Ошибка удаления item');
            }
        });
    }
}
function deleteFile(fileId) {
    if (confirm("Вы уверены, что хотите удалить этот файл?")) {
        $.ajax({
            url: `/CurrentProjects/DeleteFile?id=${fileId}`,
            type: 'POST',           
            success: function () {
                alert('Файл успешно удалён');
                location.reload();
            },
            error: function () {
                alert('Ошибка удаления файла');
            }
        });
    }
}


async function editProject(id) {
    console.log("Открытие карточки проекта РЕДАКТИРОВАНИЕ с ID:", id);
    $('#projectData').css('visibility', 'hidden');
    $('#addProject').css('visibility', 'visible');
    //$('#addProject').css('z-index', '5');
    $('.popup_before').css('visibility', 'visible');

    $.ajax({
        url: `/CurrentProjects/EditProject`,
        type: 'GET',
        data: { id: id },
        success: async function (data) {
            await $('#addProject').html(data);
            const selectedClientId = document.querySelector('#SelectedClientId').value;
            const selectedEmployeeId = document.querySelector('#SelectedEmployeeId').value;

            const selectClients = document.querySelector('#select-Clients');
            const selectEmployees = document.querySelector('#select-Employees');
            const selectMaterials = document.querySelectorAll('.select-Materials');
            const selectColors = document.querySelectorAll('.select-Colors');

            getClients(selectClients);
            getEmployees(selectEmployees);

            for (const select of selectMaterials) {
                await getMaterials(select);
            }

            for (const select of selectColors) {
                await getColors(select);
            }
            selectClients.value = selectedClientId;
            selectEmployees.value = selectedEmployeeId;

            for (let i = 0; i < selectMaterials.length; i++) {
                const selectedMaterials = document.getElementById(`selectedMaterials-${i}`).value.split(',');
                const select = selectMaterials[i];
                Array.from(select.options).forEach(option => {
                    if (selectedMaterials.includes(option.value)) {
                        option.selected = true;
                    }
                });
            }
            for (let i = 0; i < selectColors.length; i++) {
                const selectedColors = document.querySelector(`#selectedColors-${i}`).value.split(',');
                const select = selectColors[i];
                Array.from(select.options).forEach(option => {
                    if (selectedColors.includes(option.value)) {
                        option.selected = true;
                    }
                });
            }
        }
    });

    $('.popup_before').click(function () {
        $('#addProject').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });
}



//проблема была в том, что обработчики загружались до появления форм на странице
// сейчас обработчик назначен в нкоплке.
// но сервер отвечает 400 на отправку данных,
// пока не разобралась



$('#addProjectForm').submit(async function (e) {
    e.preventDefault();
    console.log('#addProjectForm');
    await sendProjectData("add");
});

$('#editProjectForm').submit(async function (e) {
    e.preventDefault();
   await sendProjectData("edit");
});

async function sendProjectData(method) {
    // ---------------------------- Часть с загрузкой общей инфо проекта ---------------------------- //
    const projectData = new FormData();
    projectData.append('ProjectName', $('#projectName').val());
    projectData.append('ProjectId', $('#ProjectIdInput').val() || 0);
    projectData.append('Deadline', $('#deadLine').val());
    projectData.append('ClientId', $('#select-Clients').val());
    projectData.append('EmployeeId', $('#select-Employees').val() || 0);
    projectData.append('EmployeePayment', $('#EmployeePaymentInput').val());
    projectData.append('PaymentDate', $('#paymentDate').val());

    //// Получаем файлы проекта
    //const projectFiles = $('#addProjectForm input[type="file"]')[0].files;
    //for (let i = 0; i < projectFiles.length; i++) {
    //    projectData.append('ProjectFiles', projectFiles[i]);
    //}
    console.log("--------------проект общие--------------");
    projectData.forEach((value, key) => {
        console.log(key, value);
    });
    const projectResponse = await fetch('/CurrentProjects/AddProject', {
        method: 'POST',
        body: projectData
    });

    if (projectResponse.ok) {
        console.log('Проект инфо успешно добавлен.');
    }

    if (!projectResponse.ok) {
        throw new Error('Ошибка при загрузке информации о проекте.');
    }
    // alert('Проект успешно добавлен.');

    //---------------------------- Отправка информации об items ----------------------------//
    const items = [];
    const projectName = $('#projectName').val();
    $('#addItemTable tr').each(function () {
        const $row = $(this);


        const sketch = $row.find('input[type="file"][name^="items"]').prop('files')[0];
        const sketchPath = $row.find('[name^="items"][name*=".sketchPath"]').val();
        const itemType = $row.find('[name^="items"][name*=".itemType"]').val();
        const itemName = $row.find('[name^="items"][name*=".itemName"]').val();
        const amount = $row.find('[name^="items"][name*=".itemCount"]').val();
        const price = $row.find('[name^="items"][name*=".itemPrice"]').val();
        const itemDescription = $row.find('[name^="items"][name*=".itemDescription"]').val();
        const itemId = $row.find('[name^="items"][name*=".itemId"]').val() || null;

        const materials = $row.find('[name^="items"][name*=".materials"] option:selected').map(function () {
            return $(this).val();
        }).get().join(',');
        const colors = $row.find('[name^="items"][name*=".colors"] option:selected').map(function () {
            return $(this).val();
        }).get().join(',');
        const deadline = $row.find('[name^="items"][name*=".itemDeadline"]').val();

        items.push({
            ProjectName: projectName,
            Sketch: sketch,
            SketchPath: sketchPath,
            ItemType: itemType,
            ItemName: itemName,
            Amount: amount,
            Price: price,
            ItemDescription: itemDescription,
            Materials: materials,
            Colors: colors,
            Deadline: deadline,
            itemId: itemId
        });
    });

    const formDataItems = new FormData();
    items.forEach((item, index) => {
        Object.keys(item).forEach(key => {
            if (key === 'SketchPath' && item[key]) {
                formDataItems.append(`items[${index}].${key}`, item[key]);
            } else {
                formDataItems.append(`items[${index}].${key}`, item[key] || '');
            }
        });
    });
    console.log("--------------items--------------");
    formDataItems.forEach((value, key) => {
        console.log(key, value);
    });
    console.log(`/CurrentProjects/${method}Items?method=${method}`);
    const itemsResponse = await fetch(`/CurrentProjects/${method}Items?method=${method}`, {
        method: 'POST',
        body: formDataItems
    });
    if (itemsResponse.ok) {
        console.log('загружены items.');
    }

    if (!itemsResponse.ok) {
        throw new Error('Ошибка при загрузке информации об items.');
    }
    //alert('Информация об items успешно загружена.');
    // ---------------------------- Часть с загрузкой только макетов проекта ---------------------------- //
    const formDataFiles = new FormData();
    const fileInput = document.getElementById('ProjectFiles');
    // const projectName = $('#projectName').val();
    const files = fileInput.files;
    for (let i = 0; i < files.length; i++) {
        const file = files[i];
        formDataFiles.append(`files[${i}].FilePath`, file);
        formDataFiles.append(`files[${i}].ProjectName`, projectName);
    }
    formDataFiles.forEach((value, key) => {
        console.log(key, value);
    });

    const filesResponse = await fetch('/CurrentProjects/AddProjectFiles', {
        method: 'POST',
        body: formDataFiles
    });
    if (filesResponse.ok) {
        console.log('файлы загружены.');
    }
    if (!filesResponse.ok) {
        throw new Error('Ошибка при загрузке файлов проекта.');
    }

    //   alert('Файлы проекта успешно загружены!');

    // ---------------------------- Часть с загрузкой только документов проекта ---------------------------- //
    const formDataDocs = new FormData();
    const docsInput = document.getElementById('ProjectDocuments');
    const docs = docsInput.files;
    for (let i = 0; i < docs.length; i++) {
        const file = docs[i];
        formDataDocs.append(`docs[${i}].FilePath`, file);
        formDataDocs.append(`docs[${i}].ProjectName`, projectName);
    }
    formDataDocs.forEach((value, key) => {
        console.log(key, value);
    });

    const docsResponse = await fetch('/CurrentProjects/AddProjectDocuments', {
        method: 'POST',
        body: formDataDocs
    });
    if (docsResponse.ok) {
        console.log('доки загружено');
       // window.location.href = '/CurrentProjects/Index';
    }
    if (!docsResponse.ok) {
        throw new Error('Ошибка при загрузке файлов проекта.');
    }
    //alert('Файлы проекта успешно загружены!');
}


async function deleteProject(projectId) {
    if (confirm("Вы уверены, что хотите удалить проект?")) {
        $.ajax({
            url: `/CurrentProjects/DeleteProject?projectId=${projectId}`,
            type: 'DELETE',
            success: function () {
                alert('проект удалён');
                location.reload();
            },
            error: function () {
                alert('Ошибка удаления');
            }
        });
    }
}

//------------------------------ЗАРПЛАТУШКА-----------------------------------//

async function loadSalary(id) {
    console.log("load salary");
    $('#salaryDiv').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');

    $.ajax({
        url: `/CurrentProjects/LoadSalary`,
        type: 'GET',
        data: { id: id },
        success: function (data) {
            $('#salaryDiv').html(data);
        },
        error: function () {
            alert("Ошибка при загрузке данных проекта.");

        }
    });
    $('.popup_before').click(function () {
        $('#salaryDiv').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });
}
async function CloseMonth(id) {
    if (confirm("Точно закрыть месяц?")) {
        console.log("CloseMonth: " + id);
        $.ajax({
            url: `/CurrentProjects/CloseMonth`,
            type: 'POST',
            data: { id: id },
            success: function () {
                alert('Месяц закрыт');
                location.reload();
            },
            error: function () {
                alert('Ошибка');
            }
        });
    }

}
function savePunishments(button, id) {
    // Найти ближайшую таблицу с классом 'items' относительно кнопки
    var table = button.closest('.client-details').querySelector('.items');
    var rows = table.querySelectorAll('tbody tr');

    var data = Array.from(rows).map(row => {
        return {
            projectPaymentId: row.dataset.projectPaymentId,
            punishment: row.querySelector('input[type="number"]').value || null, 
            punishmentDescription: row.querySelector('input[type="text"]').value || null,
            employeeId: id
        };
    });
    console.log(data);

    // Отправка данных на сервер через fetch или AJAX
    fetch('/CurrentProjects/SavePunishments', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (response.ok) {
                alert('Данные успешно сохранены!');
            } else {
                alert('Ошибка при сохранении данных.');
            }
        })
        .catch(error => {
            console.error('Ошибка:', error);
            alert('Ошибка при сохранении данных.');
        });
}

//----------------------------архивация-----------------------------------..

function authorizeYandex() {
    const clientId = "8b27fa746fe34d4991472201331608de";
    const redirectUri = "https://localhost:7189/ArchiveProjects/AddAccountYandex"; // Должен быть в настройках приложения Яндекса
    const authUrl = `https://oauth.yandex.ru/authorize?response_type=code&client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&force_confirm=yes`;

    window.location.href = authUrl;
}

async function archiveProject(id)
{
    console.log(` project id [${id}] archive request`);

    //const response = await fetch('/ArchiveProjects/ArchiveProject', {
    //    method: 'POST',
    //    data: { id: id },
    //});

    //if (response.ok) {
    //    console.log(` project id [${id}] archived sucsessfully`);
    //}

    $.ajax({
        url: `/ArchiveProjects/ArchiveProject`,
        type: 'GET',
        data: { projectId: id },
        success: function (response) {
            console.log(` project id [${id}] archived sucsessfully`);
            console.log(response.responseText);
            alert(response); 
          //  window.location.href = "/Home/Index";
        },
        error: function (response) {
            console.log(response.responseText);
            alert(response.responseText);
        }
    });
}


async function DownloadAndExtractProjectAsync(id) {
    console.log(` project id [${id}] download from archive request`);

    $.ajax({
        url: `/ArchiveProjects/DownloadAndExtractProjectAsync`,
        type: 'GET',
        data: { projectId: id },
        success: function (response) {
            console.log(` project id [${id}] dearchived sucsessfully`);
            alert(response); 
        },
        error: function (response) {
            let errorMessage = response.responseText || "Ошибка при разархивировании проекта.";
            alert(response); 
        }
    });
}