// Enum Mappings
export const TaskType = Object.freeze({
    ASSIGNMENT: 0,
    EXAM: 1
});

export const TaskPriority = Object.freeze({
    LOW: 0,
    MEDIUM: 1,
    HIGH: 2
});

export const TaskStatus = Object.freeze({
    PENDING: 0,
    COMPLETED: 1,
    CANCELLED: 2
});

export const DayOfWeek = Object.freeze({
    SATURDAY: 0,
    SUNDAY: 1,
    MONDAY: 2,
    TUESDAY: 3,
    WEDNESDAY: 4,
    THURSDAY: 5,
    FRIDAY: 6
});

// Persian Labels Mappings for UI
export const TaskTypeLabels = {
    [TaskType.ASSIGNMENT]: "تکلیف",
    [TaskType.EXAM]: "آزمون / امتحان"
};

export const TaskPriorityLabels = {
    [TaskPriority.LOW]: "پایین",
    [TaskPriority.MEDIUM]: "متوسط",
    [TaskPriority.HIGH]: "بالا"
};

export const TaskStatusLabels = {
    [TaskStatus.PENDING]: "در انتظار",
    [TaskStatus.COMPLETED]: "تکمیل شده",
    [TaskStatus.CANCELLED]: "لغو شده"
};

export const DayOfWeekLabels = {
    [DayOfWeek.SATURDAY]: "شنبه",
    [DayOfWeek.SUNDAY]: "یکشنبه",
    [DayOfWeek.MONDAY]: "دوشنبه",
    [DayOfWeek.TUESDAY]: "سه‌شنبه",
    [DayOfWeek.WEDNESDAY]: "چهارشنبه",
    [DayOfWeek.THURSDAY]: "پنج‌شنبه",
    [DayOfWeek.FRIDAY]: "جمعه"
};