export default function () {
    const validateEmail = function (rule: any, value: any, callback: any): any {
        var emailRegExp = /^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(\.[a-zA-Z0-9_-])+/;
        var emailRegExp1 = /^([a-zA-Z]|[0-9])(\w|\-)+@[a-zA-Z0-9]+\.([a-zA-Z]{2,4})$/;
        if ((!emailRegExp.test(value) && value !== '') || (!emailRegExp1.test(value) && value !== '')) {
            callback(new Error('邮箱格式不正确'));
        } else {
            callback();
        }
    }

    return { validateEmail }
}