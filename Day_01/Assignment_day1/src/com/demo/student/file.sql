1. Query : Display top 5 highest salary earners in each department

Ans : 
SELECT e.emp_id, e.emp_name, e.dept_id, e.salary
FROM employees e
WHERE (
    SELECT COUNT(*)
    FROM employees e2
    WHERE e2.dept_id = e.dept_id AND e2.salary > e.salary
) < 5
ORDER BY e.dept_id, e.salary DESC;

===========================================================================

2. Stored Function : getWorkingDays

Ans :
DELIMITER //
CREATE FUNCTION getWorkingDays(joining_date DATE)
RETURNS INT
DETERMINISTIC
BEGIN
    DECLARE total_days INT;
    SET total_days = DATEDIFF(CURDATE(), joining_date);
    RETURN total_days;
END;
//
DELIMITER ;

===========================================================================

3. Stored Procedure : updateSalaryBasedOnWorkingDays

Ans :
DELIMITER //
CREATE PROCEDURE updateSalaryBasedOnWorkingDays(IN empId INT)
BEGIN
    DECLARE days INT;
    DECLARE currentSal DECIMAL(10,2);

    SELECT getWorkingDays(joining_date) INTO days FROM employees WHERE emp_id = empId;
    SELECT salary INTO currentSal FROM employees WHERE emp_id = empId;

    IF days > 365 THEN
        SET currentSal = currentSal * 1.10;  -- 10% rise for >1 year
    ELSEIF days > 180 THEN
        SET currentSal = currentSal * 1.05;  -- 5% rise for >6 months
    ELSE
        SET currentSal = currentSal * 1.02;  -- 2% rise for new employees
    END IF;

    UPDATE employees SET salary = currentSal WHERE emp_id = empId;
END;
//
DELIMITER ;
