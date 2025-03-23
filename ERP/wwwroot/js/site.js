//const { data } = require("jquery");


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

let dataList = [];
let currentType = "";
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
                employeeCardButton.className = "btn";
                employeeCardButton.onclick = () => loadEmployeeCard(employee.employeeId);
                employeeDiv.appendChild(employeeCardButton);
            }
        
            const planButton = document.createElement("button");
            planButton.textContent = "планирование задач";
            planButton.className = "btn";
            planButton.onclick = () => taskPlanner(employee.employeeId); 
            employeeDiv.appendChild(planButton);

               

            if (isBoss || currentUserId === employee.employeeId)
            {
                const salaryButton = document.createElement("button");
                salaryButton.textContent = "ЗП архив | прогноз";
                salaryButton.className = "btn";
                salaryButton.onclick = () => loadSalary(employee.employeeId);
                employeeDiv.appendChild(salaryButton);
            }

            // Контейнер для проектов сотрудника
            const projectList = document.createElement("div");
            projectList.className = "master_list"; // Класс можно менять в зависимости от статуса
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

        if (isBoss) {
            const employeesContainer = document.getElementById("employeesContainer");

            const emplDiv = document.createElement("div");
            emplDiv.className = "header";
            const addEmplButton = document.createElement("button");
            addEmplButton.textContent = "Добавить сотрудника";
            /* addEmplButton.className = "btn";*/
            addEmplButton.onclick = () => AddEmployee();
            emplDiv.appendChild(addEmplButton);

            const clientDiv = document.createElement("div");
            clientDiv.className = "header";
            const addClientButton = document.createElement("button");
            addClientButton.textContent = "Добавить заказчика";
            /* addClientButton.className = "btn";*/
            addClientButton.onclick = () => AddClient();
            clientDiv.appendChild(addClientButton);

            employeesContainer.appendChild(emplDiv);
            employeesContainer.appendChild(clientDiv);


            const redactDiv = document.createElement("div");
            redactDiv.className = "header";
            const redactMaterials = document.createElement("button");
            redactMaterials.textContent = "Редактировать материалы";          
            redactMaterials.onclick = () => RedactType("Materials");
            redactDiv.appendChild(redactMaterials);

            const redactColors = document.createElement("button");
            redactColors.textContent = "Редактировать цвета";
            redactColors.onclick = () => RedactType("Colors");
            redactDiv.appendChild(redactColors);

            employeesContainer.appendChild(redactDiv);
        }
    } catch (error) {
        console.error("Ошибка при загрузке данных:", error);
    }
}
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

    $('#calendarContainerForeign').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');
    $('.popup_before').click(function () {
        $('#calendarContainerForeign').css('visibility', 'hidden');
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


///---------------------------------------полноэкранная галерея---------------------------------------//


function openGallery(image) {
    var gallery = image.closest('.gallery');
    var images = gallery.querySelectorAll('img');
    console.log(gallery);
    console.log(images);

    // Фильтруем картинки, убирая те, у которых есть класс "plus"
    var imagePaths = Array.from(images)
        .filter(img => !img.classList.contains('plus'))
        .map(img => img.src);

    var imageData = Array.from(images)
        .filter(img => !img.classList.contains('plus'))
        .map(img => img.dataset.fileId);


    var modal = document.createElement('div');
    modal.classList.add('modal-gallery');
    Object.assign(modal.style, {
        position: 'fixed', top: 0, left: 0, width: '100%', height: '100%',
        backgroundColor: 'rgba(0, 0, 0, 0.8)', display: 'flex',
        alignItems: 'center', justifyContent: 'center', zIndex: 1000
    });

    var modalImage = document.createElement('img');
    modalImage.src = image.src;
    Object.assign(modalImage.style, {
        maxWidth: '90%', maxHeight: '90%', objectFit: 'contain'
    });

    // SVG-кнопки (стрелки)
    function createSvgButton(direction) {
        var button = document.createElement('button');
        button.innerHTML = `<svg width="40" height="40" viewBox="0 0 24 24" fill="white">
            <path d="${direction === 'left' ? 'M15 5l-7 7 7 7' : 'M9 5l7 7-7 7'}" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>`;
        Object.assign(button.style, {
            position: 'absolute', top: '50%', transform: 'translateY(-50%)',
            background: 'transparent', border: 'none', cursor: 'pointer'
        });
        return button;
    }

    var prevButton = createSvgButton('left');
    prevButton.style.left = '20px';
    prevButton.style.width = 'min-content';
    var nextButton = createSvgButton('right');
    nextButton.style.right = '20px';
    nextButton.style.width = 'min-content';

    var closeButton = document.createElement('button');
    closeButton.innerHTML = `<svg width="30" height="30" viewBox="0 0 24 24" fill="white">
        <path d="M18 6L6 18M6 6l12 12" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
    </svg>`;
    Object.assign(closeButton.style, {
        position: 'absolute', top: '15px', right: '15px', background: 'transparent',
        border: 'none', cursor: 'pointer'
    });

    var deleteButton = document.createElement('button');
    deleteButton.innerHTML = `<svg width="30" height="30" viewBox="0 0 24 24" fill="white">
        <path d="M3 6h18M9 6v12m6-12v12M5 6l1 14h12l1-14" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
    </svg>`;
    Object.assign(deleteButton.style, {
        position: 'absolute', bottom: '20px', background: 'transparent',
        border: 'none', cursor: 'pointer'
    });

    modal.appendChild(modalImage);
    modal.appendChild(prevButton);
    modal.appendChild(nextButton);
    modal.appendChild(closeButton);

    var currentIndex = imagePaths.indexOf(modalImage.src);
    var originalImage = Array.from(gallery.querySelectorAll('img'))
        .find(img => img.src === modalImage.src);

    if (!originalImage.classList.contains('itemImage')) {
        modal.appendChild(deleteButton);
    }
    document.body.appendChild(modal);

    var currentIndex = imagePaths.indexOf(modalImage.src);

    function showImage(index) {
        if (index < 0) index = imagePaths.length - 1;
        else if (index >= imagePaths.length) index = 0;
        modalImage.src = imagePaths[index];
        currentIndex = index;
    }

    prevButton.addEventListener('click', () => showImage(currentIndex - 1));
    nextButton.addEventListener('click', () => showImage(currentIndex + 1));

    closeButton.addEventListener('click', () => document.body.removeChild(modal));
    modal.addEventListener('click', (event) => {
        if (event.target === modal) document.body.removeChild(modal);
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') document.body.removeChild(modal);
    });

    deleteButton.addEventListener('click', function () {
        var fileId = imageData[currentIndex];
        console.log(imageData);
        console.log(currentIndex);
        console.log(fileId);

        fetch(`/CurrentProjects/DeletePhoto?id=${fileId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
        }).then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('потречено');

                    imagePaths.splice(currentIndex, 1);
                    imageData.splice(currentIndex, 1);

                    if (imagePaths.length === 0) {
                        document.body.removeChild(modal);
                        return;
                    }
                    document.body.removeChild(modal);

                    var allImages = Array.from(gallery.querySelectorAll('img'));
                    var targetImage = allImages.find(img => img.src === modalImage.src);

                    if (targetImage) {
                        targetImage.remove();

                        currentIndex = Math.max(0, currentIndex - 1);
                        showImage(currentIndex);
                    } else {
                        alert('Ошибка удаления');
                    }
                }
            });
    });
}
async function plusClick(fileType, id, noteId) {

    // event.stopPropagation();

    console.log(fileType);
    console.log(id);
    console.log(noteId);

    await $.get("/CurrentProjects/AddPhotosPartial", function (data) {
        $("#addImageInner").html(data); // Вставляем форму в DOM
    });

    //$('#addPhotoProjectId').val(id);
    //$('#addPhotoNoteId').val(noteId);
    //$('#addPhotoFileType').val(fileType);


    $('#addImage').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');
    $('.popup_before').css('z-index', '3');
   // $('#addImage').find('#addPhotoFileType').val(fileType);  

    $('#addImage').find('#addPhotoNoteId').val(noteId);
    $('#addImage').find('#addPhotoProjectId').val(id);
    $('#addImage').find('#addPhotoFileType').val(fileType);





    let select = $('#addImage').find('#itemSelect');
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

    $('.popup_before').click(function () {
        $('#addImage').css('visibility', 'hidden');
        $('#popup_before').css('visibility', 'hidden');
        $('.popup_before').css('z-index', '1');
    });
}


//-----------------------------------отправка формы с фото проекта, журнала---------------------------//
///--------------------------------------заметки журнала---------------------------------------//

    function toggleJournalNotes(button) {
        var journalNotes = document.getElementById("journal-notes");
    var isVisible = journalNotes.style.display !== "none";

    journalNotes.style.display = isVisible ? "none" : "block";
    button.innerHTML = isVisible
    ? `<svg width="30" height="30" viewBox="0 0 24 24" fill="white">
        <path d="M6 15l6-6 6 6" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
    </svg>`
    : `<svg width="30" height="30" viewBox="0 0 24 24" fill="white">
        <path d="M6 9l6 6 6-6" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
    </svg>`;
    }
    async function addJournalNote() {
  //  var journalNotesContainer = document.getElementById("journal-notes");
        //var addButton = document.querySelector(".add-note-btn");

        var addButton = event.currentTarget;
        console.log(addButton);

        var journalNotesContainer = addButton.closest(".journal-notes");
        console.log(journalNotesContainer);


    var journalContainer = document.createElement("div");
    journalContainer.classList.add("journal-container");

    var journalContainer = document.createElement("div");
        journalContainer.classList.add("journal-container");

    var hiddenInput = document.createElement("input");
        hiddenInput.setAttribute("type", "hidden");

    hiddenInput.setAttribute("id", "noteId");
    hiddenInput.setAttribute("value", "0");
    journalContainer.appendChild(hiddenInput);

    // Верхняя строка с темой и кнопкой удаления
    var journalHeader = document.createElement("div");
    journalHeader.classList.add("journal-header");

    var label = document.createElement("label");
    label.textContent = "Тема:";

    var select = document.createElement("select");
        select.classList.add("topic-select");


    try
    {
        let response = await fetch('/CurrentProjects/GetJournalTopics'); // Запрос тем
        let topics = await response.json();

        console.log(topics);

        topics.forEach(topic => {
            var option = document.createElement("option");
        option.value = topic.journalTopicId;
        option.textContent = topic.journalTopicName;
        select.appendChild(option);
        });
    } catch (error) {
        console.error("Ошибка загрузки тем:", error);
    }

    var deleteButton = document.createElement("button");
    deleteButton.classList.add("delete-note-btn");
    deleteButton.setAttribute("title", "Удалить");
    deleteButton.setAttribute("onclick", "deleteJournalNote(this)");

    var deleteIcon = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    deleteIcon.setAttribute("width", "24");
    deleteIcon.setAttribute("height", "24");
    deleteIcon.setAttribute("viewBox", "0 0 24 24");

    var deletePath = document.createElementNS("http://www.w3.org/2000/svg", "path");
    deletePath.setAttribute("d", "M3 6h18M9 6v12m6-12v12M5 6l1 14h12l1-14");
    deletePath.setAttribute("stroke", "white");
    deletePath.setAttribute("stroke-width", "2");
    deletePath.setAttribute("stroke-linecap", "round");
    deletePath.setAttribute("stroke-linejoin", "round");

    deleteIcon.appendChild(deletePath);
    deleteButton.appendChild(deleteIcon);

    journalHeader.appendChild(label);
    journalHeader.appendChild(select);
    journalHeader.appendChild(deleteButton);

    // Контейнер для текста и фото
    var journalContent = document.createElement("div");
    journalContent.classList.add("journal-content");

    var textBlock = document.createElement("div");
    textBlock.classList.add("text_block");

    var textarea = document.createElement("textarea");
        textarea.setAttribute("oninput", "textareaResize(this)");
        textarea.setAttribute("onblur", "sendCardText('PutProjectNote', null)");
    textBlock.appendChild(textarea);

    var gallery = document.createElement("div");
    gallery.classList.add("journal_images", "gallery");

    var plusImg = document.createElement("img");
    plusImg.classList.add("plus");
    plusImg.setAttribute("src", "/plus.jpg"); // Исправленный путь
        plusImg.setAttribute("alt", "Добавить фото");


  
    var projectIdElement = document.querySelector('#projectId');
    var projectId = projectIdElement ? projectIdElement.value : null;

    plusImg.onclick = () => plusClick('журналФото', projectId, hiddenInput.value);

    gallery.appendChild(plusImg);

    journalContent.appendChild(textBlock);
    journalContent.appendChild(gallery);

    journalContainer.appendChild(journalHeader);
    journalContainer.appendChild(journalContent);


    journalNotesContainer.insertBefore(journalContainer, addButton);
    }
function deleteJournalNote(button) {
    var note = button.closest(".journal-container");
    let hiddenInput = note.querySelector('input[type="hidden"]');
    var noteId = hiddenInput.value; 
    if (!confirm("Удалить эту заметку?")) return;

    note.remove();
    console.log(`/CurrentProjects/DeleteJournalNote?id=${noteId}`);
    // Если у заметки есть ID, отправляем запрос на сервер
    if (noteId) {
        fetch(`/CurrentProjects/DeleteJournalNote?id=${noteId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    console.log("Заметка удалена с сервера");
                } else {
                    alert("Ошибка удаления заметки");
                }
            })
            .catch(error => console.error("Ошибка:", error));
        }
    }


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

//document.addEventListener("DOMContentLoaded", () => {
//    const textarea = document.getElementsByTagName("textarea");
//    textarea.forEach( t => textareaResize(t)); // Подстроить высоту при загрузке страницы
//});

//не ресайзится изначально суука
function textareaResize(textarea) {
    /*textarea.style.height = "auto"; */
    textarea.style.height = textarea.scrollHeight + "px";
}
function sendCardText(action, noteId) {
    const textarea = event.target;
    const description = textarea.value;
    let container = textarea.closest(".journal-container"); // Ищем родительский .journal-container
    let topicSelect = container.querySelector(".topic-select"); // Находим первый select в этом контейнере


    console.log(container);
    console.log(topicSelect);

    let selectedTopicId = topicSelect ? topicSelect.value : null;

    //var projectIdElement = document.querySelector('#projectId');
    //let projectInput = container.querySelector(".note-projectId"); 
    //let projectInputData = projectInput.dataset.projectId;
    //console.log(projectInputData);

    //var projectId = projectIdElement ? projectIdElement.value : (projectInputData ? projectInputData : null);


    var projectIdElement = document.querySelector('#projectId');
    var projectId;

    if (projectIdElement) {
        projectId = projectIdElement.value;
    } else {
        let projectInput = container.querySelector(".note-projectId");
        let projectInputData = projectInput ? projectInput.dataset.projectId : null;
        projectId = projectInputData ? projectInputData : null;
    }

    console.log(projectId);



    console.log("sendCardText: " + description);
  //  console.log(JSON.stringify({ description }));

    console.log(`/CurrentProjects/${action}?id=${noteId}&topicId=${selectedTopicId}&projectId=${projectId}`);

    fetch(`/CurrentProjects/${action}?id=${noteId}&topicId=${selectedTopicId}&projectId=${projectId}`, {
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
            return response.json();
        })
        .then(data => {
            console.log("Успешно отправлено:", data);

            // Если заметка новая и у неё не было ID, обновляем скрытый input
            if (!noteId && data.noteId) {
                var note = textarea.closest(".journal-container");
                let hiddenInput = note.querySelector('input[type="hidden"]');

                hiddenInput.value = data.noteId;
                console.log("Присвоен новый ID заметке:", data.noteId);
            }
        })
        .catch(error => {
            console.error("Ошибка:", error);
        });
}



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


//document.getElementById("photoForm").addEventListener("submit", async function (event) {
//    event.preventDefault();

//    const projectId = document.getElementById("addPhotoProjectId").value;
//    const noteId = document.getElementById("addPhotoNoteId").value;
//    const fileType = document.getElementById("addPhotoFileType").value;
//    const itemPointer = document.getElementById("itemSelect").value;
//    const photos = document.querySelector('input[type="file"]').files;

//    const formData = new FormData();
//    const fileInput = document.getElementById('AddPhotosInput');
//    formData.append(`projectId`, projectId);
//    formData.append(`noteId`, noteId);
//    formData.append(`fileType`, fileType);
//    formData.append(`itemPointer`, itemPointer);

//    const files = fileInput.files;
//    for (let i = 0; i < files.length; i++) {
//        const file = files[i];
//        formData.append(`photos[${i}].photo`, file);
//        formData.append(`photos[${i}].projectId`, projectId);
//        formData.append(`photos[${i}].noteId`, noteId);
//        formData.append(`photos[${i}].fileType`, fileType);
//        formData.append(`photos[${i}].itemPointer`, itemPointer);
//    }


//    formData.forEach((value, key) => {
//        console.log(key, value);
//    });

//  //  await delay(15000);

//    const itemsResponse = await fetch('/CurrentProjects/AddPhotos', {
//        method: 'POST',
//        body: formData
//    });
//    if (itemsResponse.ok) {
//        alert("загружено");
//        let gallery;
//        if (fileType == "журналФото") {
//            gallery = document.querySelector(`.journal-container input[value="${noteId}"]`)?.closest('.journal-container')?.querySelector('.gallery');
//        } else {
//            gallery = document.querySelector('.gallery-photo');
//        }

//        if (gallery) {
//            for (let i = 0; i < files.length; i++) {
//                const file = files[i];

//                // Создаём объект URL для превью (без запроса к серверу)
//                const imgURL = URL.createObjectURL(file);

//                const img = document.createElement("img");
//                img.src = imgURL;
//                img.alt = file.name;
//                img.dataset.fileName = file.name;
//                img.onclick = function () { openGallery(img); };

//                gallery.appendChild(img);
//            }
//        }

//        $('#addImage').css('visibility', 'hidden');
//        $('.popup_before').css('z-index', '1');
//    }
//});



async function AddPhotos() {

        const projectId = document.getElementById("addPhotoProjectId").value;
        const noteId = document.getElementById("addPhotoNoteId").value;
        const fileType = document.getElementById("addPhotoFileType").value;
        const itemPointer = document.getElementById("itemSelect").value;
        const photos = document.querySelector('input[type="file"]').files;

        const formData = new FormData();
        const fileInput = document.getElementById('AddPhotosInput');
        formData.append(`projectId`, projectId);
        formData.append(`noteId`, noteId);
        formData.append(`fileType`, fileType);
        formData.append(`itemPointer`, itemPointer);

        const files = fileInput.files;
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            formData.append(`photos[${i}].photo`, file);
            formData.append(`photos[${i}].projectId`, projectId);
            formData.append(`photos[${i}].noteId`, noteId);
            formData.append(`photos[${i}].fileType`, fileType);
            formData.append(`photos[${i}].itemPointer`, itemPointer);
        }


        formData.forEach((value, key) => {
            console.log(key, value);
        });

        //  await delay(15000); 

        const itemsResponse = await fetch('/CurrentProjects/AddPhotos', {
            method: 'POST',
            body: formData
        });
        if (itemsResponse.ok) {
            alert("загружено");
            let gallery;
            if (fileType == "журналФото") {
                gallery = document.querySelector(`.journal-container input[value="${noteId}"]`)?.closest('.journal-container')?.querySelector('.gallery');
            } else {
                gallery = document.querySelector('.gallery-photo');
            }

            if (gallery) {
                for (let i = 0; i < files.length; i++) {
                    const file = files[i];

                    // Создаём объект URL для превью (без запроса к серверу)
                    const imgURL = URL.createObjectURL(file);

                    const img = document.createElement("img");
                    img.src = imgURL;
                    img.alt = file.name;
                    img.dataset.fileName = file.name;
                    img.onclick = function () { openGallery(img); };

                    gallery.appendChild(img);
                }
            }

            $('#addImage').css('visibility', 'hidden');
            $('.popup_before').css('visibility', 'hidden');
            $('.popup_before').css('z-index', '1');
        }

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

    $('.popup_before').click(function () {
        $('#addClient').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
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
async function editClientDivOpen(id) {
    let editClient = $('#editClient');
    $('#editClient').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');
   // $('.popup_before').css('z-index', '3');

    //editClient.innerHTML = ` <svg class="close-btn" onclick="closeDiv('editClient')" xmlns="http://www.w3.org/2000/svg" width="24" height="24"
    //     viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round"
    //     stroke-linejoin="round">
    //    <line x1="18" y1="6" x2="6" y2="18"></line>
    //    <line x1="6" y1="6" x2="18" y2="18"></line>
    //</svg>
    //<div id="editClientInner" class="inner-container"></div>`;

    let div = $('#editClientInner');
   
    await $.get('/CurrentProjects/EditClientPartial', { clientId: id }, function (data) {
        //console.log(data);
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
    //$('.close-btn').click(function () {
    //    $('#editClient').css('visibility', 'hidden');
    //    $('.popup_before').css('visibility', 'hidden');
    //    $('.popup_before').css('z-index', '1');
    //});
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
                location.reload();
            },
            error: function () {
                alert('Ошибка');
            }
        });
    }
}

//-------------------------------------------- Employee -----------------------------------------------||


function AddEmployee() {
    $('#addEmployee').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');
    $('.popup_before').css('z-index', '3');

    $.get("/CurrentProjects/CreateEmployeePartial", function (data) {
        $("#addEmployeeInner").html(data);
    });
   
    $('.popup_before').click(function () {
        $('#addEmployee').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });

}
async function loadEmployeeCard(id) {
    //console.log("Открытие карточки сотрудника ID:", id);

    $('#employeeData').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');

    $.ajax({
        url: `/CurrentProjects/LoadEmployeeCard`,
        type: 'GET',
        data: { id: id },
        success: async function (data) {
            await $('#employeeDataInner').html(data);
        }
    });

    $('.popup_before').click(function () {
        $('#employeeData').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });
}

function onEmployeeAdded(response) {
    alert(response.message); // Показываем alert с текстом от сервера
    $("#employeeForm")[0].reset(); // Очищаем форму после успешного добавления
}

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
function removeItemRow(button) {
        const row = button.closest('tr'); 
        row.remove(); 
}

//document.querySelector('#fileInputFirst').addEventListener('change', function () {
//    const file = this.files[0];
//    if (file) {
//        const reader = new FileReader();
//        const img = document.querySelector('#imgFirst');
//        reader.onload = function (e) {
//            img.src = e.target.result;
//        };
//        reader.readAsDataURL(file);
//    }
//});
function showInput(selectId) {
    const inputContainer = document.getElementById(`input-${selectId}`);
    inputContainer.style.display = 'block'; // Показать блок с инпутом
}
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

function addProject() {
    $('#addProject').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');

    $.ajax({
        url: `/CurrentProjects/CreateProjectPartial`,
        type: 'GET',
        success: async function (data) {
            await $('#addProjectInner').html(data);
            getColors(document.getElementById('select-Colors'));
            getMaterials(document.getElementById('select-Materials'));
            getEmployees(document.getElementById('select-Employees'));
            getClients(document.getElementById('select-Clients'));
        }
    });

    $('.popup_before').click(function () {
        $('#addProject').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });
}
function openProjectCard(id) {
    console.log("Открытие карточки проекта с ID:", id);
    $('#projectData').css('visibility', 'visible');
   // $('#projectDataInner').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');


   

    $.ajax({
        url: `/LoadProjectCard`,
        type: 'GET',
        data: { id: id },
        success: function (data) {
            $('#projectDataInner').html(data);
        },
        error: function () {
            alert("Ошибка при загрузке данных проекта.");
        }
    });
    //$('#projectData .close-btn').on("click", function () {
    //    $('#projectData').css('visibility', 'hidden');
    //    $('.popup_before').css('visibility', 'hidden');
    //    $('.popup_before').css('z-index', '1');
    //});


    $('.popup_before').click(function () {
        $('#projectData').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });
}
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
async function deleteItem(id, but) {
    if (confirm("Вы уверены, что хотите удалить этот Item?")) {
        $.ajax({
            url: `/CurrentProjects/DeleteItem?id=${id}`,
            type: 'POST',
            success: function () {
                alert('item успешно удалён');
                const row = but.closest('tr');
                row.remove();  
                //location.reload();
            },
            error: function () {
                alert('Ошибка удаления item');
            }
        });
    }
}
function deleteFile(fileId, but) {
    if (confirm("Вы уверены, что хотите удалить этот файл?")) {
        $.ajax({
            url: `/CurrentProjects/DeleteFile?id=${fileId}`,
            type: 'POST',           
            success: function () {
                alert('Файл успешно удалён');
                const row = but.closest('tr');
                row.remove();  
                //location.reload();
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


    $('.close-btn').click(function () {
        $('#addProject').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
        $('.popup_before').css('z-index', '1');
    });

    $.ajax({
        url: `/CurrentProjects/EditProject`,
        type: 'GET',
        data: { id: id },
        success: async function (data) {
            await $('#addProjectInner').html(data);
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
    projectData.append('LayoutsRequired', $('#layoutsRequired').val());

    if ($('#isDocumentsComleted').prop('checked')) {
        projectData.append('IsDocumentsComleted', true);
    } else {
        projectData.append('IsDocumentsComleted', false);
    }

    

    //// Получаем файлы проекта
    //const projectFiles = $('#addProjectForm input[type="file"]')[0].files;
    //for (let i = 0; i < projectFiles.length; i++) {
    //    projectData.append('ProjectFiles', projectFiles[i]);
    //}
   // console.log("--------------проект общие--------------");
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
      //  window.location.href = '/CurrentProjects/Index';
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
            $('#salaryDivInner').html(data);
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

async function authorizeYandex() {
    try {
        const response = await fetch("/ArchiveProjects/GetConfig");
        const data = await response.json();

        if (!data.clientId) {
            console.error("Ошибка: client_id не получен.");
            return;
        }

        const authUrl = `https://oauth.yandex.ru/authorize?response_type=code&client_id=${data.clientId}&redirect_uri=${encodeURIComponent(data.redirectUri)}&force_confirm=yes`;

        window.location.href = authUrl;
    } catch (error) {
        console.error("Ошибка при получении client_id:", error);
    }
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
            location.reload();
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
            alert("разархивировано"); 
            location.reload();
        },
        error: function (response) {
            let errorMessage = response.responseText || "Ошибка при разархивировании проекта.";
            alert(errorMessage); 
        }
    });
}
function setActiveAccount(accountId) {

    console.log("setActiveAccount:  " + accountId)


    $.ajax({
        url: `/ArchiveProjects/SetActiveAccount`,
        type: 'POST',
        data: { accountId: accountId },
        success: function (data) {
            console.log("Аккаунт успешно активирован!");
            alert("сменили аккаунт");
        },
        error: function (data) {
            alert("Ошибка:", data.message);
        }
    });


    //fetch(`/ArchiveProjects/SetActiveAccount?id=${accountId}`, {
    //    method: "POST",
    //    headers: {
    //        "Content-Type": "application/json",
    //        "X-CSRF-TOKEN": document.querySelector('input[name="__RequestVerificationToken"]')?.value
    //    },
    //    body: JSON.stringify({ accountId: accountId })
    //})
    //    .then(response => response.json())
    //    .then(data => {
    //        if (data.success) {
    //            console.log("Аккаунт успешно активирован!");
    //            alert("сменили аккаунт");
    //        } else {
    //            console.error("Ошибка:", data.message);
    //        }
    //    })
    //    .catch(error => console.error("Ошибка запроса:", error));
}


//---------------------------------цвета и материалы---------------------------------------//

function RedactType(type) {
    const popup = document.getElementById("editPopup");
    const popup_before = document.querySelector(".popup_before");

    currentType = "";
    currentType = type;
    popup.style.visibility = "visible";
    popup_before.style.visibility = "visible";

    $('.popup_before').click(function () {
        $('#editPopup').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });

    $('#searchInput').on("change", () => typeOptionClick($('#searchInput').val()));
    fetchData(type);
}
async function fetchData(type) {

    const response = await fetch(`/CurrentProjects/Get${type}`);
    dataList = await response.json();
    console.log(dataList);
    updateSelectOptions(type);
}
function typeOptionClick(id) {
    console.log("typeOptionClick");
    console.log(id);
    let typeId = document.querySelector("#typeId");
    typeId.value = id;
    typeId.text = id;

}
function updateSelectOptions(type) {
    const searchText = document.getElementById("searchInput").value.toLowerCase();
    document.querySelector("#typeId").innerHTML = "";
    const selectElement = document.getElementById("editSelect");
    selectElement.innerHTML = "";
    console.log(type);
    if (type === "Materials") {
        console.log("type = Materials")
        dataList.filter(item => item.materialName.toLowerCase().startsWith(searchText))
            .forEach(item => {
                const option = document.createElement("option");
                option.value = item.materialName;
                selectElement.appendChild(option);
            });
    }

    if (type === "Colors") {
        console.log("type = Colors")
        dataList.filter(item => item.colorName.toLowerCase().startsWith(searchText))
            .forEach(item => {
                const option = document.createElement("option");
                option.value = item.colorName;

                selectElement.appendChild(option);
            });
    }
}
async function saveTypeChanges() {
    //  const selectElement = document.getElementById("editSelect");
    // const inputField = document.getElementById("searchInput");
    const selectedType = document.getElementById("searchInput").value;
    const newName = document.querySelector("#typeId").value;

    console.log(`/CurrentProjects/Update${currentType}?name=${selectedType}&newName=${newName}`);

    if (!selectedType || !newName) return;

    const response = await fetch(`/CurrentProjects/Update${currentType}?name=${selectedType}&newName=${newName}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" }
        //  body: JSON.stringify({ id: selectedId, name: newName })
    });
    if (response.ok) {
        location.reload();
    }
    else {
        //const responseData = response.json();
        alert("Ошибка:", response.message);
    }
}
async function deleteTypeItem() {
    // const selectElement = document.getElementById("editSelect");
    const selectedType = document.querySelector("#typeId").value;

    if (!selectedType) return;

    console.log(`/CurrentProjects/Delete${currentType}?name=${selectedType}`);

    const response = await fetch(`/CurrentProjects/Delete${currentType}?name=${selectedType}`, { method: "DELETE" });
    if (response.ok) {
        location.reload();
    }
    else {
        //const responseData = response.json();
        alert("Ошибка:", response.message);
    }
}



//---------------------------------прочее---------------------------------------//

function closeDiv(div) {
    console.log("closeDiv" + div);
    $('#' + div).css('visibility', 'hidden');
    console.log(`#${div}`);
   // $('#'+id).css('visibility', 'hidden');
    $('.popup_before').css('visibility', 'hidden');
}


function addJournalTopic() {
 
    $('#addTopic').css('visibility', 'visible');
    $('.popup_before').css('visibility', 'visible');

    $.get("/Journal/AddTopicPartial", function (data) {
        $("#addTopicInner").html(data);
    });
    

    $('.popup_before').click(function () {
        $('#editPopup').css('visibility', 'hidden');
        $('.popup_before').css('visibility', 'hidden');
    });
}
