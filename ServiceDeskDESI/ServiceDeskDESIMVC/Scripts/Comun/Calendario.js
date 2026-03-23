document.addEventListener('DOMContentLoaded', function () {
    // Obtener fecha actual para la semana
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');

    // Fechas para la semana actual
    const fechaHoy = `${year}-${month}-${day}`;

    // Calcular fecha de mañana
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    const tomorrowYear = tomorrow.getFullYear();
    const tomorrowMonth = String(tomorrow.getMonth() + 1).padStart(2, '0');
    const tomorrowDay = String(tomorrow.getDate()).padStart(2, '0');
    const fechaManana = `${tomorrowYear}-${tomorrowMonth}-${tomorrowDay}`;

    // Calcular fecha de ayer
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);
    const yesterdayYear = yesterday.getFullYear();
    const yesterdayMonth = String(yesterday.getMonth() + 1).padStart(2, '0');
    const yesterdayDay = String(yesterday.getDate()).padStart(2, '0');
    const fechaAyer = `${yesterdayYear}-${yesterdayMonth}-${yesterdayDay}`;

    // Calcular fecha dentro de 2 días
    const twoDaysLater = new Date(today);
    twoDaysLater.setDate(twoDaysLater.getDate() + 2);
    const twoDaysYear = twoDaysLater.getFullYear();
    const twoDaysMonth = String(twoDaysLater.getMonth() + 1).padStart(2, '0');
    const twoDaysDay = String(twoDaysLater.getDate()).padStart(2, '0');
    const fecha2Dias = `${twoDaysYear}-${twoDaysMonth}-${twoDaysDay}`;

    // Datos de ejemplo para el calendario (JSON simulado desde servicio)
    const eventos = [
        {
            id: '1',
            title: 'Mantenimiento de servidores',
            start: fechaHoy,
            end: fechaManana,
            color: '#e74a3b',
            description: 'Mantenimiento preventivo de servidores principales'
        },
        {
            id: '2',
            title: 'Reunión de equipo',
            start: fechaHoy + 'T10:30:00',
            end: fechaHoy + 'T12:30:00',
            color: '#4e73df',
            description: 'Reunión semanal de seguimiento'
        },
        {
            id: '3',
            title: 'Capacitación nuevos agentes',
            start: fechaManana,
            end: fecha2Dias,
            color: '#1cc88a',
            description: 'Entrenamiento para nuevos agentes de soporte'
        },
        {
            id: '4',
            title: 'Vencimiento de tickets críticos',
            start: fecha2Dias,
            color: '#f6c23e',
            description: 'Tickets con prioridad alta por vencer'
        },
        {
            id: '5',
            title: 'Revisión de SLA',
            start: fechaAyer,
            color: '#36b9cc',
            description: 'Revisión de acuerdos de nivel de servicio'
        },
        {
            id: '6',
            title: 'Implementación de actualización',
            start: fechaHoy,
            color: '#9b59b6',
            description: 'Actualización de parches de seguridad'
        }
    ];

    // Inicializar calendario con eventos arrastrables
    const calendarEl = document.getElementById('calendar');
    const calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        locale: 'es',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        events: eventos,
        editable: true, // Permite arrastrar eventos
        eventDragStart: function (info) {
            console.log('Comenzando a arrastrar:', info.event.title);
        },
        eventDrop: function (info) {
            // Mostrar confirmación con SweetAlert2
            Swal.fire({
                title: '¿Cambiar fecha del evento?',
                text: `¿Estás seguro que quieres mover "${info.event.title}" a otra fecha?`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#4e73df',
                cancelButtonColor: '#e74a3b',
                confirmButtonText: 'Sí, cambiar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Aquí iría la llamada al servicio para actualizar la fecha
                    console.log('Evento movido:', info.event.title);
                    console.log('Nueva fecha:', info.event.start);

                    Swal.fire(
                        '¡Cambiado!',
                        'La fecha del evento ha sido actualizada.',
                        'success'
                    );
                } else {
                    // Revertir el cambio
                    info.revert();
                    Swal.fire(
                        'Cancelado',
                        'El evento no ha sido modificado.',
                        'info'
                    );
                }
            });
        },
        eventClick: function (info) {
            Swal.fire({
                title: info.event.title,
                html: `
                                <p><strong>Fecha:</strong> ${info.event.start.toLocaleDateString('es-MX')}</p>
                                <p><strong>Descripción:</strong> ${info.event.extendedProps.description || 'Sin descripción'}</p>
                            `,
                icon: 'info',
                confirmButtonColor: '#4e73df'
            });
        }
    });

    calendar.render();

    // =========================================
    // GRÁFICAS CON CHART.JS
    // =========================================

    // Gráfica de tickets por estado
    const ctx1 = document.getElementById('ticketsChart').getContext('2d');
    new Chart(ctx1, {
        type: 'doughnut',
        data: {
            labels: ['Abiertos', 'En Proceso', 'Resueltos', 'Cerrados'],
            datasets: [{
                data: [45, 32, 68, 54],
                backgroundColor: [
                    '#e74a3b',
                    '#f6c23e',
                    '#1cc88a',
                    '#36b9cc'
                ],
                borderWidth: 0
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });

    // Gráfica de tickets por prioridad
    const ctx2 = document.getElementById('priorityChart').getContext('2d');
    new Chart(ctx2, {
        type: 'bar',
        data: {
            labels: ['Alta', 'Media', 'Baja'],
            datasets: [{
                label: 'Tickets',
                data: [28, 45, 19],
                backgroundColor: [
                    '#e74a3b',
                    '#f6c23e',
                    '#1cc88a'
                ],
                borderWidth: 0
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });

    // =========================================
    // FUNCIONALIDAD DEL SIDEBAR
    // =========================================
    document.getElementById('sidebarCollapse').addEventListener('click', function () {
        document.getElementById('sidebar').classList.toggle('active');
        document.getElementById('content').classList.toggle('active');
    });

    // Manejar submenús
    document.querySelectorAll('.has-arrow').forEach(item => {
        item.addEventListener('click', function (e) {
            e.preventDefault();
            const parent = this.parentElement;
            parent.classList.toggle('active');
        });
    });
});