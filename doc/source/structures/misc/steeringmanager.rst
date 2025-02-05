.. _steeringmanager:

SteeringManager
===============

See :ref:`the cooked control tuning explanation <cooked_tuning>` for
information to help with tuning the steering manager.  It's important to read
that section first to understand which setting below is affecting which
portion of the steering system.

The SteeringManager is a bound variable, not a suffix to a specific vessel.  This prevents access to the SteeringManager of other vessels.  You can access the steering manager as shown below: ::

    // Display the ship facing, target facing, and world coordinates vectors.
    SET STEERINGMANAGER:SHOWFACINGVECTORS TO TRUE.

    // Change the torque calculation to multiply the available torque by 1.5.
    SET STEERINGMANAGER:ROLLTORQUEFACTOR TO 1.5.

.. note::

    .. versionadded:: 0.18

        The :struct:`SteeringManager` was added to improve the accuracy of kOS's cooked steering.  While this code is a significant improvement over the old system, it is not perfect.  Specifically it does not properly calculate the effects of control surfaces, nor does it account for atmospheric drag.  It also does not adjust for asymmetric RCS or Engine thrust.  It does allow for some modifications to the built in logic through the torque adjustments and factors.  However, if there is a condition for which the new steering manager is unable to provide accurate control, you should continue to fall back to raw controls.


.. structure:: SteeringManager

    ===================================== ========================= =============
     Suffix                                Type                      Description
    ===================================== ========================= =============
     :attr:`PITCHPID`                      :struct:`PIDLoop`         The PIDLoop for the pitch :ref:`rotational velocity PID <cooked_omega_pid>`.
     :attr:`YAWPID`                        :struct:`PIDLoop`         The PIDLoop for the yaw :ref:`rotational velocity PID <cooked_omega_pid>`.
     :attr:`ROLLPID`                       :struct:`PIDLoop`         The PIDLoop for the roll :ref:`rotational velocity PID <cooked_omega_pid>`.
     :attr:`ENABLED`                       bool                      Returns true if the `SteeringManager` is currently controlling the vessel
     :attr:`TARGET`                        :struct:`Direction`       The direction that the vessel is currently steering towards
     :meth:`RESETPIDS()`                   none                      Called to call `RESET` on all steering PID loops.
     :attr:`SHOWFACINGVECTORS`             bool                      Enable/disable display of ship facing, target, and world coordinates vectors.
     :attr:`SHOWANGULARVECTORS`            bool                      Enable/disable display of angular rotation vectors
     :attr:`SHOWTHRUSTVECTORS`             bool                      Enable/disable display of engine thrust vectors
     :attr:`SHOWRCSVECTORS`                bool                      Enable/disable display of rcs thrust vectors
     :attr:`SHOWSTEERINGSTATS`             bool                      Enable/disable printing of the steering information on the terminal
     :attr:`WRITECSVFILES`                  bool                      Enable/disable logging steering to csv files.
     :attr:`PITCHTS`                       scalar (s)                Settling time for the pitch torque calculation.
     :attr:`YAWTS`                         scalar (s)                Settling time for the yaw torque calculation.
     :attr:`ROLLTS`                        scalar (s)                Settling time for the roll torque calculation.
     :attr:`MAXSTOPPINGTIME`               scalar (s)                The maximum amount of stopping time to limit angular turn rate.
     :attr:`ANGLEERROR`                    scalar (deg)              The angle between vessel:facing and target directions
     :attr:`PITCHERROR`                    scalar (deg)              The angular error in the pitch direction
     :attr:`YAWERROR`                      scalar (deg)              The angular error in the yaw direction
     :attr:`ROLLERROR`                     scalar (deg)              The angular error in the roll direction
     :attr:`PITCHTORQUEADJUST`             scalar (kN)               Additive adjustment to pitch torque (calculated)
     :attr:`YAWTORQUEADJUST`               scalar (kN)               Additive adjustment to yaw torque (calculated)
     :attr:`ROLLTORQUEADJUST`              scalar (kN)               Additive adjustment to roll torque (calculated)
     :attr:`PITCHTORQUEFACTOR`             scalar                    Multiplicative adjustment to pitch torque (calculated)
     :attr:`YAWTORQUEFACTOR`               scalar                    Multiplicative adjustment to yaw torque (calculated)
     :attr:`ROLLTORQUEFACTOR`              scalar                    Multiplicative adjustment to roll torque (calculated)
    ===================================== ========================= =============

.. attribute:: SteeringManager:PITCHPID

    :type: :struct:`PIDLoop`
    :access: Get only

    Returns the PIDLoop object responsible for calculating the :ref:`target angular velocity <cooked_omega_pid>` in the pitch direction.  This allows direct manipulation of the gain parameters, and other components of the :struct:`PIDLoop` structure.  Changing the loop's `MAXOUTPUT` or `MINOUTPUT` values will have no effect as they are overwritten every physics frame.  They are set to limit the maximum turning rate to that which can be stopped in a :attr:`MAXSTOPPINGTIME` seconds (calculated based on available torque, and the ship's moment of inertia).

.. attribute:: SteeringManager:YAWPID

    :type: :struct:`PIDLoop`
    :access: Get only

    Returns the PIDLoop object responsible for calculating the :ref:`target angular velocity <cooked_omega_pid>` in the yaw direction.  This allows direct manipulation of the gain parameters, and other components of the :struct:`PIDLoop` structure.  Changing the loop's `MAXOUTPUT` or `MINOUTPUT` values will have no effect as they are overwritten every physics frame.  They are set to limit the maximum turning rate to that which can be stopped in a :attr:`MAXSTOPPINGTIME` seconds (calculated based on available torque, and the ship's moment of inertia).

.. attribute:: SteeringManager:ROLLPID

    :type: :struct:`PIDLoop`
    :access: Get only

    Returns the PIDLoop object responsible for calculating the :ref:`target angular velocity <cooked_omega_pid>` in the roll direction.  This allows direct manipulation of the gain parameters, and other components of the :struct:`PIDLoop` structure.  Changing the loop's `MAXOUTPUT` or `MINOUTPUT` values will have no effect as they are overwritten every physics frame.  They are set to limit the maximum turning rate to that which can be stopped in a :attr:`MAXSTOPPINGTIME` seconds (calculated based on available torque, and the ship's moment of inertia).

    .. note::

        The SteeringManager will ignore the roll component of steering
        until after both the pitch and yaw components are close to being
        correct.  In other words it will try to point the nose of the
        craft in the right direction first, before it makes any attempt
        to roll the craft into the right orientation.  As long as the
        pitch or yaw is still far off from the target aim, this PIDloop
        won't be getting used at all.

.. attribute:: SteeringManager:ENABLED

    :type: bool
    :access: Get only

    Returns true if the SteeringManager is currently controlling the vessel steering.

.. attribute:: SteeringManager:TARGET

    :type: :struct:`Direction`
    :access: Get only

    Returns direction that the is currently being targeted.  If steering is locked to a vector, this will return the calculated direction in which kOS chose an arbitrary roll to go with the vector.  If steering is locked to "kill", this will return the vessel's last facing direction.

.. method:: SteeringManager:RESETPIDS

    :return: none

    Resets the integral sum to zero for all six steering PID Loops.

.. attribute:: SteeringManager:SHOWFACINGVECTORS

    :type: bool
    :access: Get/Set

    Setting this suffix to true will cause the steering manager to display graphical vectors (see :struct:`VecDraw`) representing the forward, top, and starboard of the facing direction, as well as the world x, y, and z axis orientation (centered on the vessel).  Setting to false will hide the vectors, as will disabling locked steering.

.. attribute:: SteeringManager:SHOWANGULARVECTORS

    :type: bool
    :access: Get/Set

    Setting this suffix to true will cause the steering manager to display graphical vectors (see :struct:`VecDraw`) representing the current and target angular velocities in the pitch, yaw, and roll directions.  Setting to false will hide the vectors, as will disabling locked steering.

.. attribute:: SteeringManager:SHOWTHRUSTVECTORS

    :type: bool
    :access: Get/Set

    Setting this suffix to true will cause the steering manager to display graphical vectors (see :struct:`VecDraw`) representing the thrust and torque for each active engine.  Setting to false will hide the vectors, as will disabling locked steering.

.. attribute:: SteeringManager:SHOWRCSVECTORS

    :type: bool
    :access: Get/Set

    Setting this suffix to true will cause the steering manager to display graphical vectors (see :struct:`VecDraw`) representing the thrust and torque for each active RCS block.  Setting to false will hide the vectors, as will disabling locked steering.

.. attribute:: SteeringManager:SHOWSTEERINGSTATS

    :type: bool
    :access: Get/Set

    Setting this suffix to true will cause the steering manager to clear the terminal screen and print steering data each update.

.. attribute:: SteeringManager:WRITECSVFILES

    :type: bool
    :access: Get/Set

    Setting this suffix to true will cause the steering manager log the data from all 6 PIDLoops calculating target angular velocity and target torque.  The files are stored in the `[KSP Root]\GameData\kOS\Plugins\PluginData\kOS` folder, with one file per loop and a new file created for each new manager instance (i.e. every launch, every revert, and every vessel load).  These files can grow quite large, and add up quickly, so it is recommended to only set this value to true for testing or debugging and not normal operation.

.. attribute:: SteeringManager:PITCHTS

    :type: scalar
    :access: Get/Set

    Represents the settling time for the :ref:`PID calculating pitch torque based on target angular velocity <cooked_torque_pid>`.  The proportional and integral gain is calculated based on the settling time and the moment of inertia in the pitch direction.  Ki = (moment of inertia) * (4 / (settling time)) ^ 2.  Kp = 2 * sqrt((moment of inertia) * Ki).

.. attribute:: SteeringManager:YAWTS

    :type: scalar
    :access: Get/Set

    Represents the settling time for the :ref:`PID calculating yaw torque based on target angular velocity <cooked_torque_pid>`.  The proportional and integral gain is calculated based on the settling time and the moment of inertia in the yaw direction.  Ki = (moment of inertia) * (4 / (settling time)) ^ 2.  Kp = 2 * sqrt((moment of inertia) * Ki).

.. attribute:: SteeringManager:ROLLTS

    :type: scalar
    :access: Get/Set

    Represents the settling time for the :ref:`PID calculating roll torque based on target angular velocity <cooked_torque_pid>`.  The proportional and integral gain is calculated based on the settling time and the moment of inertia in the roll direction.  Ki = (moment of inertia) * (4 / (settling time)) ^ 2.  Kp = 2 * sqrt((moment of inertia) * Ki).

.. attribute:: SteeringManager:MAXSTOPPINGTIME

    :type: scalar (s)
    :access: Get/Set

    This value is used to limit the turning rate when :ref:`calculating target angular velocity <cooked_omega_pid>`.  The ship will not turn faster than what it can stop in this amount of time.  The maximum angular velocity about each axis is calculated as: (max angular velocity) = MAXSTOPPINGTIME * (available torque) / (moment of inertia).

    .. note::

        This setting affects all three of the :ref:`rotational velocity PID's <cooked_omega_pid>` at once (pitch, yaw, and roll), rather than affecting the three axes individually one at a time.

.. attribute:: SteeringManager:ANGLEERROR

    :type: scalar (deg)
    :access: Get only

    The angle between the ship's facing direction forward vector and the target direction's forward.  This is the combined pitch and yaw error.

.. attribute:: SteeringManager:PITCHERROR

    :type: scalar (deg)
    :access: Get only

    The pitch angle between the ship's facing direction and the target direction.

.. attribute:: SteeringManager:YAWERROR

    :type: scalar (deg)
    :access: Get only

    The yaw angle between the ship's facing direction and the target direction.

.. attribute:: SteeringManager:ROLLERROR

    :type: scalar (deg)
    :access: Get only

    The roll angle between the ship's facing direction and the target direction.

.. attribute:: SteeringManager:PITCHTORQUEADJUST

    :type: scalar (kNm)
    :access: Get/Set

    You can set this value to provide an additive bias to the calculated available pitch torque used in the pitch :ref:`torque PID <cooked_torque_pid>`. (available torque) = ((calculated torque) + PITCHTORQUEADJUST) * PITCHTORQUEFACTOR.

.. attribute:: SteeringManager:YAWTORQUEADJUST

    :type: scalar (kNm)
    :access: Get/Set

    You can set this value to provide an additive bias to the calculated available yaw torque used in the yaw :ref:`torque PID <cooked_torque_pid>`. (available torque) = ((calculated torque) + YAWTORQUEADJUST) * YAWTORQUEFACTOR.

.. attribute:: SteeringManager:ROLLTORQUEADJUST

    :type: scalar (kNm)
    :access: Get/Set

    You can set this value to provide an additive bias to the calculated available roll torque used in the roll :ref:`torque PID <cooked_torque_pid>`. (available torque) = ((calculated torque) + ROLLTORQUEADJUST) * ROLLTORQUEFACTOR.

.. attribute:: SteeringManager:PITCHTORQUEFACTOR

    :type: scalar (kNm)
    :access: Get/Set

    You can set this value to provide an multiplicative factor bias to the calculated available pitch torque used in the :ref:`torque PID <cooked_torque_pid>`. (available torque) = ((calculated torque) + PITCHTORQUEADJUST) * PITCHTORQUEFACTOR.

.. attribute:: SteeringManager:YAWTORQUEFACTOR

    :type: scalar (kNm)
    :access: Get/Set

    You can set this value to provide an multiplicative factor bias to the calculated available yaw torque used in the :ref:`torque PID <cooked_torque_pid>`. (available torque) = ((calculated torque) + YAWTORQUEADJUST) * YAWTORQUEFACTOR.

.. attribute:: SteeringManager:ROLLTORQUEFACTOR

    :type: scalar (kNm)
    :access: Get/Set

    You can set this value to provide an multiplicative factor bias to the calculated available roll torque used in the :ref:`torque PID <cooked_torque_pid>`. (available torque) = ((calculated torque) + ROLLTORQUEADJUST) * ROLLTORQUEFACTOR.
