//
// Authors:
//   刘静谊 (Johnny Liu) <jingeelio@163.com>
//
// Copyright (c) 2017 刘静谊 (Johnny Liu)
//
// Licensed under the LGPLv3 license. Please see <http://www.gnu.org/licenses/lgpl-3.0.html> for license text.
//

using System;

#if ENCRYPT
namespace JointCode.Internals
#else
namespace JointCode.Common.ExceptionHandling
#endif
{
    /// 首先应该知道，异常用于在执行条件不满足时，及时中断当前程序执行流，避免出现更大的错误。
    /// 而异常处理机制（抛出异常）的目的则在于让应用程序决定是否能够从异常中恢复并继续执行；
    /// 如果不能恢复，也可以优雅地退出应用程序（例如进行一些异常记录），而不是简单粗暴地中断应用程序。
    /// 我们可以简单将异常分为以下几种类型：
    /// 1. 不会对内部状态造成破坏的异常。在 JointCode 的代码中，除了继承自 <see cref="FatalException"/> 和 <see cref="LogicalException"/> 之外的
    ///    异常都属于此类。例如，参数错误、IO 错误等异常。对于这类异常，应用程序可以从异常中恢复并继续执行。
    /// 2. 可能会对内部状态造成破坏的异常，在 JointCode 的代码中，对应于继承自 <see cref="LogicalException"/> 的异常。例如，无效操作、数据错误等
    ///    异常。对于这类异常，应用程序可以选择从异常中恢复并继续执行，也可以直接中断应用程序执行。
    /// 3. 肯定会对内部状态造成破坏的异常，在 JointCode 的代码中，对应于继承自 <see cref="FatalException"/> 的异常。例如，资源耗竭、堆栈溢出、访问
    ///    违规、等异常。对于这类异常，应用程序不应从异常中恢复，而应中断应用程序运行。
    /// 我们可以简单将异常分为以下几种类型：
    /// 1.不会对内部状态造成破坏的异常。例如，参数错误、IO 错误等异常。对于这类异常，应用程序可以从异常中恢复并继续执行。
    /// 2.可能会对内部状态造成破坏的异常。例如，无效操作、数据错误等异常。对于这类异常，应用程序可以选择从异常中恢复并继续执行，也可以直接中断应用程序执行。
    /// 3.肯定会对内部状态造成破坏的异常。例如，资源耗竭、堆栈溢出、访问违规、等异常。对于这类异常，应用程序不应从异常中恢复，而应直接中断应用程序运行。

    /// 不同类型的异常应该继承自不同的异常基类，以便能够通过 <see cref="IExceptionManager"/> 和 <see cref="IExceptionHandler"/> 进行适当的处理。


    /// <see cref="IExceptionManager"/> 的目的在于通过配置（可以是代码配置或者通过配置文件进行配置）来实现不同异常类型的处理策略，即能够根据不同的
    /// 异常类型创建不同的 <see cref="IExceptionHandler"/> 进行异常处理。

    /// 从处理机制的角度来讲，异常处理可以从三个维度进行解析：
    /// 1. 异常出现的位置（也就是在哪个类的哪个方法中出现了异常）：因为不同代码的重要性是不一样的，有些代码是任务关键性的（例如银行的资金交易代码），
    ///    有些代码只是一般的业务逻辑。因此，它们出现异常时，要求的处理流程也可能会有所不同。例如，对于关键代码可能需要在第一时间通知核心代码开发者，
    ///    还可能需要通知主管领导，以及时进行修补。而对于一般业务逻辑的异常，则仅仅可能是记录一下日志就可以了。
    /// 2. 异常的类型：通过一些策略设置，可以根据异常类型来决定应用程序是否可以自行恢复并继续执行。
    /// 3. 出现异常时的环境信息（例如操作系统名称和版本、CPU 占用情况等）：此部分信息的捕捉应放在日志模块完成。
    public interface IExceptionManager
    {
        IExceptionHandler GetExceptionHandler(Type type);
        IExceptionHandler GetExceptionHandler(string handlerName);
    }
}